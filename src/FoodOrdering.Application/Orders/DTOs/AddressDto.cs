namespace FoodOrdering.Application.Orders.DTOs;

/// <summary>
/// Data Transfer Object for Address.
/// </summary>
public record AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public static AddressDto FromAddress(Domain.Ordering.ValueObjects.Address address)
    {
        return new AddressDto
        {
            Street = address.Street,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }
}
