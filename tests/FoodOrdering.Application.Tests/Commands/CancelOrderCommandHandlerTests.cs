using FluentAssertions;
using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.Commands.CancelOrder;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.Repositories;
using NSubstitute;
using Xunit;

namespace FoodOrdering.Application.Tests.Commands;

public class CancelOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _restaurantRepository = Substitute.For<IRestaurantRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CancelOrderCommandHandler(
            _orderRepository,
            _restaurantRepository,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_WithValidPendingOrder_ShouldCancelOrder()
    {
        // Arrange
        var order = CreatePendingOrder();
        var restaurant = CreateTestRestaurant(order.RestaurantId);
        restaurant.IncrementPendingOrders();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new CancelOrderCommand
        {
            OrderId = order.Id.Value,
            Reason = "Changed my mind"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Cancelled");
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Changed my mind");

        _orderRepository.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldDecrementRestaurantPendingOrders()
    {
        // Arrange
        var order = CreatePendingOrder();
        var restaurant = CreateTestRestaurant(order.RestaurantId);
        restaurant.IncrementPendingOrders();
        restaurant.IncrementPendingOrders();
        var initialCount = restaurant.PendingOrderCount;

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);
        _restaurantRepository.GetByIdAsync(Arg.Any<RestaurantId>())
            .Returns(restaurant);

        var command = new CancelOrderCommand
        {
            OrderId = order.Id.Value,
            Reason = "Test reason"
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        restaurant.PendingOrderCount.Should().Be(initialCount - 1);
        _restaurantRepository.Received(1).Update(restaurant);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns((Order?)null);

        var command = new CancelOrderCommand
        {
            OrderId = Guid.NewGuid(),
            Reason = "Test reason"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WithDeliveredOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateDeliveredOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new CancelOrderCommand
        {
            OrderId = order.Id.Value,
            Reason = "Want to cancel"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be cancelled");
    }

    [Fact]
    public async Task Handle_WithOutForDeliveryOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateOutForDeliveryOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new CancelOrderCommand
        {
            OrderId = order.Id.Value,
            Reason = "Too late"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be cancelled");
    }

    [Fact]
    public async Task Handle_WithEmptyReason_ShouldReturnFailure()
    {
        // Arrange
        var order = CreatePendingOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new CancelOrderCommand
        {
            OrderId = order.Id.Value,
            Reason = "" // Empty reason
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #region Helper Methods

    private static Order CreatePendingOrder()
    {
        var order = Order.Create(
            CustomerId.New(),
            RestaurantId.New(),
            new Address("123 Test St", "Kuala Lumpur", "WP", "50000", "Malaysia")
        );

        order.AddItem(
            MenuItemId.New(),
            "Test Item",
            new Money(10m, "MYR"),
            1
        );

        order.Place();
        return order;
    }

    private static Order CreateOutForDeliveryOrder()
    {
        var order = CreatePendingOrder();
        order.Confirm(DateTime.UtcNow.AddMinutes(20));
        order.StartPreparing();
        order.MarkReadyForPickup();
        order.PickUp();
        return order;
    }

    private static Order CreateDeliveredOrder()
    {
        var order = CreateOutForDeliveryOrder();
        order.Deliver();
        return order;
    }

    private static Restaurant CreateTestRestaurant(RestaurantId restaurantId)
    {
        // Note: In real scenario, you'd use reflection or a test builder
        // to create a restaurant with a specific ID
        return Restaurant.Create(
            "Test Restaurant",
            "Test Description",
            new Address("789 Restaurant St", "Kuala Lumpur", "WP", "50200", "Malaysia")
        );
    }

    #endregion
}