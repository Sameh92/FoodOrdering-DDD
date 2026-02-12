namespace FoodOrdering.API.DTOs.RestaurantDTOs;

public record SetAvailabilityRequest
{
    public bool IsAvailable { get; init; }
}