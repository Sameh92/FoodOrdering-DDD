namespace FoodOrdering.API.DTOs.RestaurantDTOs;

public record UpdateRestaurantRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
