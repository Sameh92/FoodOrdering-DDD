using FluentAssertions;
using FoodOrdering.Application.Orders.Queries.GetCustomerOrders;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using NSubstitute;
using Xunit;

namespace FoodOrdering.Application.Tests.Queries;

public class GetCustomerOrdersQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly GetCustomerOrdersQueryHandler _handler;

    public GetCustomerOrdersQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _handler = new GetCustomerOrdersQueryHandler(_orderRepository);
    }

    [Fact]
    public async Task Handle_WithExistingOrders_ShouldReturnOrders()
    {
        // Arrange
        var customerId = CustomerId.New();
        var orders = new List<Order>
        {
            CreateTestOrder(customerId),
            CreateTestOrder(customerId),
            CreateTestOrder(customerId)
        };

        _orderRepository.GetByCustomerIdAsync(Arg.Any<CustomerId>())
            .Returns(orders);

        var query = new GetCustomerOrdersQuery
        {
            CustomerId = customerId.Value
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        _orderRepository.GetByCustomerIdAsync(Arg.Any<CustomerId>())
            .Returns(new List<Order>());

        var query = new GetCustomerOrdersQuery
        {
            CustomerId = Guid.NewGuid()
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapOrderSummaryProperties()
    {
        // Arrange
        var customerId = CustomerId.New();
        var order = CreateTestOrder(customerId);
        order.Place();

        _orderRepository.GetByCustomerIdAsync(Arg.Any<CustomerId>())
            .Returns(new List<Order> { order });

        var query = new GetCustomerOrdersQuery
        {
            CustomerId = customerId.Value
        };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var summary = result.Value!.First();
        summary.Id.Should().Be(order.Id.Value);
        summary.Status.Should().Be("PendingConfirmation");
        summary.ItemCount.Should().Be(2); // Quantity of 2
    }

    #region Helper Methods

    private static Order CreateTestOrder(CustomerId customerId)
    {
        var order = Order.Create(
            customerId,
            RestaurantId.New(),
            new Address("123 Test St", "Kuala Lumpur", "WP", "50000", "Malaysia")
        );

        order.AddItem(
            MenuItemId.New(),
            "Test Item",
            new Money(10m, "MYR"),
            2
        );

        return order;
    }

    #endregion
}