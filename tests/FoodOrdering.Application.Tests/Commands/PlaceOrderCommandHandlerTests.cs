using FluentAssertions;
using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.Commands.PlaceOrder;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.Repositories;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Ordering.Services;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.Repositories;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using NSubstitute;
using Xunit;

namespace FoodOrdering.Application.Tests.Commands;

public class PlaceOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DeliveryFeeCalculator _deliveryFeeCalculator;
    private readonly RestaurantAvailabilityService _availabilityService;
    private readonly PlaceOrderCommandHandler _handler;

    public PlaceOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _customerRepository = Substitute.For<ICustomerRepository>();
        _restaurantRepository = Substitute.For<IRestaurantRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _deliveryFeeCalculator = new DeliveryFeeCalculator();
        _availabilityService = new RestaurantAvailabilityService();

        _handler = new PlaceOrderCommandHandler(
            _orderRepository,
            _customerRepository,
            _restaurantRepository,           
            _deliveryFeeCalculator,
            _availabilityService,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();
        var menuItem = restaurant.MenuItems.First();

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new()
                {
                    MenuItemId = menuItem.Id.Value,
                    Quantity = 2
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be("PendingConfirmation");
        result.Value.Items.Should().HaveCount(1);

        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleItems_ShouldCreateOrderWithAllItems()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var menuItems = restaurant.MenuItems.Take(2).ToList();

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = menuItems.Select(m => new OrderItemRequest
            {
                MenuItemId = m.Id.Value,
                Quantity = 1
            }).ToList()
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldReturnFailure()
    {
        // Arrange
        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns((Customer?)null);

        var command = new PlaceOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            RestaurantId = Guid.NewGuid(),
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Customer");
    }

    [Fact]
    public async Task Handle_WithNonExistentRestaurant_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateTestCustomer();

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns((Restaurant?)null);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = Guid.NewGuid(),
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Restaurant");
    }

    [Fact]
    public async Task Handle_WithInactiveRestaurant_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();
        restaurant.Deactivate();

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = restaurant.MenuItems.First().Id.Value, Quantity = 1 }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not accepting orders");
    }

    [Fact]
    public async Task Handle_WithUnavailableMenuItem_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();
        var menuItem = restaurant.MenuItems.First();
        restaurant.SetMenuItemAvailability(menuItem.Id, false);

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = menuItem.Id.Value, Quantity = 1 }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not available");
    }

    [Fact]
    public async Task Handle_WithEmptyItems_ShouldReturnFailure()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>() // Empty items
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("item");
    }

    [Fact]
    public async Task Handle_ShouldIncrementRestaurantPendingOrders()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();
        var initialPendingOrders = restaurant.PendingOrderCount;

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = restaurant.MenuItems.First().Id.Value, Quantity = 1 }
            }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        restaurant.PendingOrderCount.Should().Be(initialPendingOrders + 1);
        _restaurantRepository.Received(1).Update(restaurant);
    }

    [Fact]
    public async Task Handle_ShouldIncrementCustomerOrderCount()
    {
        // Arrange
        var customer = CreateTestCustomer();
        var restaurant = CreateTestRestaurant();
        var initialOrderCount = customer.TotalOrders;

        _customerRepository.GetByIdAsync(Arg.Any<CustomerId>())
            .Returns(customer);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new PlaceOrderCommand
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressDto
            {
                Street = "123 Test St",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequest>
            {
                new() { MenuItemId = restaurant.MenuItems.First().Id.Value, Quantity = 1 }
            }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        customer.TotalOrders.Should().Be(initialOrderCount + 1);
        _customerRepository.Received(1).Update(customer);
    }

    #region Helper Methods

    private static Customer CreateTestCustomer()
    {
        return Customer.Create(
            new CustomerName("John", "Doe"),
            new Email("john@example.com"),
            new PhoneNumber("+60123456789"),
            new Address("456 Customer St", "Kuala Lumpur", "WP", "50100", "Malaysia")
        );
    }

    private static Restaurant CreateTestRestaurant()
    {
        var restaurant = Restaurant.Create(
            "Test Restaurant",
            "Test Description",
            new Address("789 Restaurant St", "Kuala Lumpur", "WP", "50200", "Malaysia")
        );

        restaurant.AddMenuItem(
            "Test Burger",
            "Delicious burger",
            new Money(15.90m, "MYR"),
            "Burgers",
            15
        );

        restaurant.AddMenuItem(
            "Test Fries",
            "Crispy fries",
            new Money(6.90m, "MYR"),
            "Sides",
            10
        );

        return restaurant;
    }

    #endregion
}