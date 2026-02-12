namespace FoodOrdering.API.DTOs.RestaurantDTOs;

public record RateRestaurantRequest
{
    public decimal Stars { get; init; }
}
