namespace FoodOrdering.Domain.Customers.ValueObjects;

/// <summary>
/// Phone Number Value Object.
/// </summary>
public record PhoneNumber
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number is required", nameof(value));

        // Remove spaces, dashes, and parentheses
        var cleaned = new string(value.Where(c => char.IsDigit(c) || c == '+').ToArray());

        if (cleaned.Length < 8 || cleaned.Length > 15)
            throw new ArgumentException("Invalid phone number length", nameof(value));

        Value = cleaned;
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;

    // EF Core needs this
    private PhoneNumber() { Value = string.Empty; }
}
