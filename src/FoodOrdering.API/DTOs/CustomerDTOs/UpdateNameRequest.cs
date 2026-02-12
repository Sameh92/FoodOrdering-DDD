namespace FoodOrdering.API.DTOs.CustomerDTOs;

public record UpdateNameRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
}
