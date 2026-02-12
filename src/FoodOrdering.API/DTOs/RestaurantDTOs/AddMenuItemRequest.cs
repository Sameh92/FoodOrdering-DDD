namespace FoodOrdering.API.DTOs.RestaurantDTOs;

public record AddMenuItemRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string? Currency { get; init; }
    public string Category { get; init; } = string.Empty;
    public int PreparationTimeMinutes { get; init; } = 15;
}
