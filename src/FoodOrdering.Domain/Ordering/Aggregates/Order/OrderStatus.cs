namespace FoodOrdering.Domain.Ordering.Aggregates.Order;

/// <summary>
/// Order lifecycle status.
/// Uses Ubiquitous Language - these are terms the business uses.
/// </summary>
public enum OrderStatus
{
    /// <summary>Customer is still adding items to the order</summary>
    Draft = 0,

    /// <summary>Order placed by customer, waiting for restaurant to confirm</summary>
    PendingConfirmation = 1,

    /// <summary>Restaurant confirmed the order</summary>
    Confirmed = 2,

    /// <summary>Restaurant is preparing the food</summary>
    Preparing = 3,

    /// <summary>Food is ready, waiting for driver to pick up</summary>
    ReadyForPickup = 4,

    /// <summary>Driver picked up the order, on the way to customer</summary>
    OutForDelivery = 5,

    /// <summary>Order successfully delivered to customer</summary>
    Delivered = 6,

    /// <summary>Order was cancelled</summary>
    Cancelled = 7
}

/// <summary>
/// Extension methods for OrderStatus.
/// </summary>
public static class OrderStatusExtensions
{
    /// <summary>
    /// Check if the order is in a final state (cannot be changed).
    /// </summary>
    public static bool IsFinal(this OrderStatus status)
    {
        return status == OrderStatus.Delivered || status == OrderStatus.Cancelled;
    }

    /// <summary>
    /// Check if the order can be cancelled.
    /// </summary>
    public static bool CanBeCancelled(this OrderStatus status)
    {
        return status != OrderStatus.Delivered &&
               status != OrderStatus.Cancelled &&
               status != OrderStatus.OutForDelivery;
    }

    /// <summary>
    /// Check if the order can be modified (items added/removed).
    /// </summary>
    public static bool CanBeModified(this OrderStatus status)
    {
        return status == OrderStatus.Draft;
    }

    /// <summary>
    /// Get a human-readable description of the status.
    /// </summary>
    public static string GetDescription(this OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Draft => "Order is being prepared",
            OrderStatus.PendingConfirmation => "Waiting for restaurant confirmation",
            OrderStatus.Confirmed => "Restaurant confirmed your order",
            OrderStatus.Preparing => "Your food is being prepared",
            OrderStatus.ReadyForPickup => "Ready for pickup",
            OrderStatus.OutForDelivery => "On the way to you",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            _ => "Unknown status"
        };
    }
}