using System.Text.RegularExpressions;

namespace FoodOrdering.Domain.Customers.ValueObjects;

/// <summary>
/// Email Value Object.
/// Ensures email is always in valid format.
/// </summary>
public record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required", nameof(value));

        value = value.Trim().ToLowerInvariant();

        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));

        Value = value;
    }

    private static bool IsValidEmail(string email)
    {
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return regex.IsMatch(email);
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;

    // 🔹 EF Core needs this
    private Email() { Value = string.Empty; }
}
