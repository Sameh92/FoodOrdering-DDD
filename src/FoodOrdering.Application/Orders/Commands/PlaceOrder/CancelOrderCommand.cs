using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Restaurants.Repositories;
using MediatR;

namespace FoodOrdering.Application.Orders.Commands.CancelOrder;

/// <summary>
/// Command to cancel an order.
/// </summary>
public record CancelOrderCommand : IRequest<Result<OrderDto>>
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Handler for CancelOrderCommand.
/// </summary>
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderCommandHandler(
        IOrderRepository orderRepository,
        IRestaurantRepository restaurantRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _restaurantRepository = restaurantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<OrderDto>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.From(request.OrderId);
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);

        if (order == null)
            return Result.Failure<OrderDto>("Order not found");

        if (!order.CanBeCancelled())
            return Result.Failure<OrderDto>("Order cannot be cancelled in its current state");

        try
        {
            order.Cancel(request.Reason);

            // Decrement restaurant pending order count
            var restaurant = await _restaurantRepository.GetByIdAsync(order.RestaurantId, cancellationToken);
            restaurant?.DecrementPendingOrders();

            _orderRepository.Update(order);
            if (restaurant != null)
                _restaurantRepository.Update(restaurant);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(OrderDto.FromOrder(order));
        }
        catch (Exception ex)
        {
            return Result.Failure<OrderDto>(ex.Message);
        }
    }
}