namespace FoodOrdering.API.DTOs.OrdersDTOs;

public record OrderItemRequestDto
{
    public Guid MenuItemId { get; init; }
    public int Quantity { get; init; }
    public string? SpecialInstructions { get; init; }
}
