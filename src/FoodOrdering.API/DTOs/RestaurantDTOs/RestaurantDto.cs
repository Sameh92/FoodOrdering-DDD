using FoodOrdering.API.DTOs.CustomerDTOs;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.API.DTOs.RestaurantDTOs;

// DTOs
public record RestaurantDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public CustomerAddressDto Address { get; init; } = null!;
    public decimal AverageRating { get; init; }
    public int TotalRatings { get; init; }
    public bool IsActive { get; init; }
    public int PendingOrderCount { get; init; }
    public int MaxConcurrentOrders { get; init; }
    public List<MenuItemDto> MenuItems { get; init; } = new();
    public DateTime CreatedAt { get; init; }

    public static RestaurantDto FromRestaurant(Restaurant restaurant)
    {
        return new RestaurantDto
        {
            Id = restaurant.Id.Value,
            Name = restaurant.Name,
            Description = restaurant.Description,
            Address = new CustomerAddressDto
            {
                Street = restaurant.Address.Street,
                City = restaurant.Address.City,
                State = restaurant.Address.State,
                PostalCode = restaurant.Address.PostalCode,
                Country = restaurant.Address.Country
            },
            AverageRating = restaurant.AverageRating.Stars,
            TotalRatings = restaurant.TotalRatings,
            IsActive = restaurant.IsActive,
            PendingOrderCount = restaurant.PendingOrderCount,
            MaxConcurrentOrders = restaurant.MaxConcurrentOrders,
            MenuItems = restaurant.MenuItems.Select(MenuItemDto.FromMenuItem).ToList(),
            CreatedAt = restaurant.CreatedAt
        };
    }
}
