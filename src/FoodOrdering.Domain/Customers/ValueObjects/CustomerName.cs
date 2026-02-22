namespace FoodOrdering.Domain.Customers.ValueObjects;

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