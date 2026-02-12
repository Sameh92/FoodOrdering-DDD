using FluentAssertions;
using FoodOrdering.Application.Common;
using FoodOrdering.Application.Orders.Commands.ConfirmOrder;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using NSubstitute;
using Xunit;

namespace FoodOrdering.Application.Tests.Commands;

public class ConfirmOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ConfirmOrderCommandHandler _handler;

    public ConfirmOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new ConfirmOrderCommandHandler(_orderRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WithValidPendingOrder_ShouldConfirmOrder()
    {
        // Arrange
        var order = CreatePendingOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new ConfirmOrderCommand
        {
            OrderId = order.Id.Value,
            EstimatedPrepTimeMinutes = 25
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Confirmed");
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.EstimatedReadyTime.Should().NotBeNull();

        _orderRepository.Received(1).Update(order);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDefaultPrepTime_ShouldUse20Minutes()
    {
        // Arrange
        var order = CreatePendingOrder();
        var beforeConfirm = DateTime.UtcNow;

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new ConfirmOrderCommand
        {
            OrderId = order.Id.Value,
            EstimatedPrepTimeMinutes = 20 // Default
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.EstimatedReadyTime.Should().BeOnOrAfter(beforeConfirm.AddMinutes(20));
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns((Order?)null);

        var command = new ConfirmOrderCommand
        {
            OrderId = Guid.NewGuid(),
            EstimatedPrepTimeMinutes = 20
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WithDraftOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateDraftOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new ConfirmOrderCommand
        {
            OrderId = order.Id.Value,
            EstimatedPrepTimeMinutes = 20
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithAlreadyConfirmedOrder_ShouldReturnFailure()
    {
        // Arrange
        var order = CreateConfirmedOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var command = new ConfirmOrderCommand
        {
            OrderId = order.Id.Value,
            EstimatedPrepTimeMinutes = 20
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #region Helper Methods

    private static Order CreateDraftOrder()
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

        return order;
    }

    private static Order CreatePendingOrder()
    {
        var order = CreateDraftOrder();
        order.Place();
        return order;
    }

    private static Order CreateConfirmedOrder()
    {
        var order = CreatePendingOrder();
        order.Confirm(DateTime.UtcNow.AddMinutes(20));
        return order;
    }

    #endregion
}