namespace FoodOrdering.Domain.Ordering.Aggregates.Order;

/// <summary>
/// Strongly typed ID for Order.
/// 
/// Using strongly typed IDs prevents mixing up different IDs:
/// - Can't accidentally pass a CustomerId where an OrderId is expected
/// - Compiler catches mistakes at build time
/// </summary>
public record OrderId
{
    public Guid Value { get; }

    public OrderId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty", nameof(value));

        Value = value;
    }

    public static OrderId New() => new(Guid.NewGuid());

    public static OrderId From(Guid value) => new(value);

    public static OrderId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();

    // Implicit conversion for convenience
    public static implicit operator Guid(OrderId orderId) => orderId.Value;
}