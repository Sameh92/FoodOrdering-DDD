using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.API.DTOs.RestaurantDTOs;

public record RestaurantSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public decimal AverageRating { get; init; }
    public int TotalRatings { get; init; }
    public bool IsActive { get; init; }

    public static RestaurantSummaryDto FromRestaurant(Restaurant restaurant)
    {
        return new RestaurantSummaryDto
        {
            Id = restaurant.Id.Value,
            Name = restaurant.Name,
            Description = restaurant.Description,
            City = restaurant.Address.City,
            AverageRating = restaurant.AverageRating.Stars,
            TotalRatings = restaurant.TotalRatings,
            IsActive = restaurant.IsActive
        };
    }
}
