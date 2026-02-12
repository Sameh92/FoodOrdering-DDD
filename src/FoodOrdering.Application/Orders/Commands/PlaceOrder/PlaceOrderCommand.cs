using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.Repositories;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Ordering.Services;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.Repositories;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using MediatR;

namespace FoodOrdering.Application.Orders.Commands.PlaceOrder;

/// <summary>
/// Command to place a new order.
/// </summary>
public record PlaceOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid CustomerId { get; init; }
    public Guid RestaurantId { get; init; }
    public AddressDto DeliveryAddress { get; init; } = null!;
    public List<OrderItemRequest> Items { get; init; } = new();
    public string? SpecialInstructions { get; init; }
}

/// <summary>
/// Item to add to the order.
/// </summary>
public record OrderItemRequest
{
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
    public string? SpecialInstructions { get; init; }
}

/// <summary>
/// Handler for PlaceOrderCommand.
/// 
/// This is an Application Service that:
/// - Orchestrates the use case
/// - Loads aggregates from repositories
/// - Calls domain services
/// - Calls domain methods
/// - Saves changes via Unit of Work
/// </summary>
public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly DeliveryFeeCalculator _deliveryFeeCalculator;
    private readonly RestaurantAvailabilityService _availabilityService;
    private readonly IUnitOfWork _unitOfWork;

    public PlaceOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IRestaurantRepository restaurantRepository,
        DeliveryFeeCalculator deliveryFeeCalculator,
        RestaurantAvailabilityService availabilityService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _restaurantRepository = restaurantRepository;
        _deliveryFeeCalculator = deliveryFeeCalculator;
        _availabilityService = availabilityService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            return Result.Failure<OrderDto>("Order must contain at least one item");

        // 1. Load Customer aggregate
        var customerId = CustomerId.From(request.CustomerId);
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customer == null)
            return Result.Failure<OrderDto>("Customer not found");

        // 2. Load Restaurant aggregate
        var restaurantId = RestaurantId.From(request.RestaurantId);
        var restaurant = await _restaurantRepository.GetByIdAsync(restaurantId, cancellationToken);

        if (restaurant == null)
            return Result.Failure<OrderDto>("Restaurant not found");

        // 3. Check restaurant availability using Domain Service
        if (!_availabilityService.CanAcceptOrder(restaurant, DateTime.UtcNow))
            return Result.Failure<OrderDto>("Restaurant is not accepting orders at this time");

        // 4. Create delivery address Value Object
        var deliveryAddress = new Address(
            request.DeliveryAddress.Street,
            request.DeliveryAddress.City,
            request.DeliveryAddress.State,
            request.DeliveryAddress.PostalCode,
            request.DeliveryAddress.Country
        );

        // 5. Check delivery availability using Domain Service
        if (!_deliveryFeeCalculator.IsDeliveryAvailable(restaurant.Address, deliveryAddress))
            return Result.Failure<OrderDto>("Delivery is not available to this address");

        // 6. Create Order aggregate using factory method
        var order = Order.Create(
            customerId,
            restaurantId,
            deliveryAddress,
            request.SpecialInstructions
        );

        // 7. Add items to order
        foreach (var itemRequest in request.Items)
        {
            var menuItemId = MenuItemId.From(itemRequest.MenuItemId);
            var menuItem = restaurant.GetMenuItem(menuItemId);

            if (menuItem == null)
                return Result.Failure<OrderDto>($"Menu item {itemRequest.MenuItemId} not found");

            if (!menuItem.IsAvailable)
                return Result.Failure<OrderDto>($"Menu item '{menuItem.Name}' is not available");

            order.AddItem(
                menuItemId,
                menuItem.Name,
                menuItem.Price,
                itemRequest.Quantity,
                itemRequest.SpecialInstructions
            );
        }

        // 8. Calculate delivery fee using Domain Service
        var deliveryFee = _deliveryFeeCalculator.CalculateFee(
            restaurant.Address,
            deliveryAddress,
            customer.MembershipLevel,
            DateTime.UtcNow
        );

        order.SetDeliveryFee(deliveryFee);

        // 9. Place the order (triggers domain event)
        order.Place();

        // 10. Update restaurant pending order count
        restaurant.IncrementPendingOrders();

        // 11. Update customer order count
        customer.IncrementOrderCount();

        // 12. Save to repository
        await _orderRepository.AddAsync(order, cancellationToken);
        _restaurantRepository.Update(restaurant);
        _customerRepository.Update(customer);

        // 13. Commit transaction (also dispatches domain events)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 14. Return result
        return Result.Success(OrderDto.FromOrder(order));
    }
}