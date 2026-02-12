using FluentAssertions;
using FoodOrdering.Application.Orders.Queries.GetOrderById;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using NSubstitute;
using Xunit;

namespace FoodOrdering.Application.Tests.Queries;

public class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepository);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnOrder()
    {
        // Arrange
        var order = CreateTestOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var query = new GetOrderByIdQuery
        {
            OrderId = order.Id.Value
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(order.Id.Value);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrder_ShouldReturnFailure()
    {
        // Arrange
        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns((Order?)null);

        var query = new GetOrderByIdQuery
        {
            OrderId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_ShouldMapAllOrderProperties()
    {
        // Arrange
        var order = CreateTestOrder();
        order.Place();
        order.Confirm(DateTime.UtcNow.AddMinutes(20));

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var query = new GetOrderByIdQuery
        {
            OrderId = order.Id.Value
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!;

        dto.Id.Should().Be(order.Id.Value);
        dto.CustomerId.Should().Be(order.CustomerId.Value);
        dto.RestaurantId.Should().Be(order.RestaurantId.Value);
        dto.Status.Should().Be("Confirmed");
        dto.Items.Should().HaveCount(1);
        dto.DeliveryAddress.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldMapOrderItems()
    {
        // Arrange
        var order = CreateTestOrder();

        _orderRepository.GetByIdAsync(Arg.Any<OrderId>())
            .Returns(order);

        var query = new GetOrderByIdQuery
        {
            OrderId = order.Id.Value
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var item = result.Value!.Items.First();
        item.Name.Should().Be("Test Burger");
        item.Quantity.Should().Be(2);
        item.Price.Should().Be(15.90m);
    }

    #region Helper Methods

    private static Order CreateTestOrder()
    {
        var order = Order.Create(
            CustomerId.New(),
            RestaurantId.New(),
            new Address("123 Test St", "Kuala Lumpur", "WP", "50000", "Malaysia")
        );

        order.AddItem(
            MenuItemId.New(),
            "Test Burger",
            new Money(15.90m, "MYR"),
            2
        );

        return order;
    }

    #endregion
}