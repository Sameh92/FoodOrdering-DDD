namespace FoodOrdering.API.DTOs.OrdersDTOs;

public record ConfirmOrderRequest
{
    public int EstimatedPrepTimeMinutes { get; init; } = 20;
}
