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

/// <summary>
/// Customer Name Value Object.
/// </summary>
public record CustomerName
{
    public string FirstName { get; }
    public string LastName { get; }

    public CustomerName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() => FullName;

    // EF Core needs this
    private CustomerName()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }
}