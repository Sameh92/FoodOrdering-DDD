namespace FoodOrdering.Domain.Customers.Aggregates.Customer;

/// <summary>
/// Strongly typed ID for Customer.
/// </summary>
public record CustomerId
{
    public Guid Value { get; }

    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty", nameof(value));

        Value = value;
    }

    public static CustomerId New() => new(Guid.NewGuid());

    public static CustomerId From(Guid value) => new(value);

    public static CustomerId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
}