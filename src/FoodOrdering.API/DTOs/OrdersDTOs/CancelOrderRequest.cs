namespace FoodOrdering.API.DTOs.OrdersDTOs;

public record CancelOrderRequest
{
    public string Reason { get; init; } = string.Empty;
}