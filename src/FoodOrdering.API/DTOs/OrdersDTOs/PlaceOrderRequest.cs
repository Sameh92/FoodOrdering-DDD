using FoodOrdering.API.DTOs.Common;

namespace FoodOrdering.API.DTOs.OrdersDTOs;

// Request DTOs
public record PlaceOrderRequest
{
    public Guid CustomerId { get; init; }
    public Guid RestaurantId { get; init; }
    public AddressRequest DeliveryAddress { get; init; } = null!;
    public List<OrderItemRequestDto> Items { get; init; } = new();
    public string? SpecialInstructions { get; init; }
}
