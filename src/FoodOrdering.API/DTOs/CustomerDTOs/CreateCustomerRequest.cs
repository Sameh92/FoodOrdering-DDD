using FoodOrdering.API.DTOs.Common;

namespace FoodOrdering.API.DTOs.CustomerDTOs;


public record CreateCustomerRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public AddressRequest? DefaultAddress { get; init; }
}
