namespace FoodOrdering.API.DTOs.CustomerDTOs;

public record UpdateContactRequest
{
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
