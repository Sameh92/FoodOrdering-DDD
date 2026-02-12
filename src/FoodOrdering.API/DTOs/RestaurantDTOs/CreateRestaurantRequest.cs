

using FoodOrdering.API.DTOs.Common;
using FoodOrdering.API.DTOs.CustomerDTOs;

namespace FoodOrdering.API.DTOs.RestaurantDTOs;

// Request DTOs
public record CreateRestaurantRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public AddressRequest Address { get; init; } = null!;
    public int MaxConcurrentOrders { get; init; } = 20;
}
