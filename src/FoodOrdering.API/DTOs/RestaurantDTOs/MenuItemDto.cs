using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.API.DTOs.RestaurantDTOs;

public record MenuItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "MYR";
    public string Category { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public int PreparationTimeMinutes { get; init; }

    public static MenuItemDto FromMenuItem(MenuItem menuItem)
    {
        return new MenuItemDto
        {
            Id = menuItem.Id.Value,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price.Amount,
            Currency = menuItem.Price.Currency,
            Category = menuItem.Category,
            IsAvailable = menuItem.IsAvailable,
            PreparationTimeMinutes = menuItem.PreparationTimeMinutes
        };
    }
}
