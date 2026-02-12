namespace FoodOrdering.Domain.Ordering.ValueObjects;

/// <summary>
/// Address Value Object.
/// Represents a physical address.
/// </summary>
public record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code is required", nameof(postalCode));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        State = state?.Trim() ?? string.Empty;
        PostalCode = postalCode.Trim();
        Country = country.Trim();
    }

    public string GetFullAddress()
    {
        var parts = new List<string> { Street, City };

        if (!string.IsNullOrWhiteSpace(State))
            parts.Add(State);

        parts.Add(PostalCode);
        parts.Add(Country);

        return string.Join(", ", parts);
    }

    public override string ToString() => GetFullAddress();
}