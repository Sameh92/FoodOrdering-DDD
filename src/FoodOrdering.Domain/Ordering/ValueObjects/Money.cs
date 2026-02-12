namespace FoodOrdering.Domain.Ordering.ValueObjects;

/// <summary>
/// Money Value Object.
/// Represents a monetary amount with currency.
/// 
/// This is a Value Object because:
/// - Two Money objects with same Amount and Currency are equal
/// - It's immutable (cannot change after creation)
/// - It has no identity
/// </summary>
public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));

        Amount = Math.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        var result = Amount - other.Amount;

        if (result < 0)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Factor cannot be negative", nameof(factor));

        return new Money(Amount * factor, Currency);
    }

    public Money Multiply(int factor)
    {
        return Multiply((decimal)factor);
    }

    public static Money Zero(string currency = "MYR") => new(0, currency);

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Money operator *(Money money, int factor) => money.Multiply(factor);

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException(
                $"Cannot operate on different currencies: {Currency} and {other.Currency}");
    }

    public override string ToString() => $"{Currency} {Amount:F2}";
}
