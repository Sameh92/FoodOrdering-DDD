using FoodOrdering.Domain.Ordering.Aggregates.Order;

namespace FoodOrdering.Application.Orders.DTOs;

/// <summary>
/// Data Transfer Object for OrderItem.
/// </summary>
public record OrderItemDto
{
    public Guid MenuItemId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "MYR";
    public string? SpecialInstructions { get; init; }

    public static OrderItemDto FromOrderItem(OrderItem item)
    {
        return new OrderItemDto
        {
            MenuItemId = item.MenuItemId.Value,
            Name = item.Name,
            Price = item.Price.Amount,
            Quantity = item.Quantity,
            Total = item.GetTotal().Amount,
            Currency = item.Price.Currency,
            SpecialInstructions = item.SpecialInstructions
        };
    }
}
