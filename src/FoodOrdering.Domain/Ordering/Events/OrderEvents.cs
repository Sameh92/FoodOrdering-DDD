using FoodOrdering.Domain.Common;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.Domain.Ordering.Events;

/// <summary>
/// Raised when a customer places an order.
/// 
/// Handlers might:
/// - Notify the restaurant
/// - Send confirmation email to customer
/// - Process payment
/// - Record analytics
/// </summary>
public record OrderPlacedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }
    public RestaurantId RestaurantId { get; }
    public Money Total { get; }
    public IReadOnlyList<OrderItemData> Items { get; }
    public Address DeliveryAddress { get; }
    public DateTime PlacedAt { get; }

    public OrderPlacedEvent(
        OrderId orderId,
        CustomerId customerId,
        RestaurantId restaurantId,
        Money total,
        IReadOnlyList<OrderItemData> items,
        Address deliveryAddress)
    {
        OrderId = orderId;
        CustomerId = customerId;
        RestaurantId = restaurantId;
        Total = total;
        Items = items;
        DeliveryAddress = deliveryAddress;
        PlacedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Raised when a restaurant confirms an order.
/// </summary>
public record OrderConfirmedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public RestaurantId RestaurantId { get; }
    public DateTime EstimatedReadyTime { get; }

    public OrderConfirmedEvent(
        OrderId orderId,
        RestaurantId restaurantId,
        DateTime estimatedReadyTime)
    {
        OrderId = orderId;
        RestaurantId = restaurantId;
        EstimatedReadyTime = estimatedReadyTime;
    }
}

/// <summary>
/// Raised when an order starts being prepared.
/// </summary>
public record OrderPreparationStartedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public RestaurantId RestaurantId { get; }

    public OrderPreparationStartedEvent(OrderId orderId, RestaurantId restaurantId)
    {
        OrderId = orderId;
        RestaurantId = restaurantId;
    }
}

/// <summary>
/// Raised when an order is ready for pickup.
/// </summary>
public record OrderReadyForPickupEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public RestaurantId RestaurantId { get; }

    public OrderReadyForPickupEvent(OrderId orderId, RestaurantId restaurantId)
    {
        OrderId = orderId;
        RestaurantId = restaurantId;
    }
}

/// <summary>
/// Raised when a driver picks up an order.
/// </summary>
public record OrderPickedUpEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }

    public OrderPickedUpEvent(OrderId orderId, CustomerId customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
    }
}

/// <summary>
/// Raised when an order is delivered.
/// </summary>
public record OrderDeliveredEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }
    public DateTime DeliveredAt { get; }

    public OrderDeliveredEvent(OrderId orderId, CustomerId customerId)
    {
        OrderId = orderId;
        CustomerId = customerId;
        DeliveredAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Raised when an order is cancelled.
/// </summary>
public record OrderCancelledEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public CustomerId CustomerId { get; }
    public RestaurantId RestaurantId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(
        OrderId orderId,
        CustomerId customerId,
        RestaurantId restaurantId,
        string reason)
    {
        OrderId = orderId;
        CustomerId = customerId;
        RestaurantId = restaurantId;
        Reason = reason;
    }
}

/// <summary>
/// Data transfer object for order items in events.
/// We don't expose the actual OrderItem entity.
/// </summary>
public record OrderItemData(
    MenuItemId MenuItemId,
    string Name,
    int Quantity,
    Money Price,
    string? SpecialInstructions = null
);