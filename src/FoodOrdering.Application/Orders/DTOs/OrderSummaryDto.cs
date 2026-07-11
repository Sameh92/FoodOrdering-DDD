using FoodOrdering.Domain.Ordering.Aggregates.Order;

namespace FoodOrdering.Application.Orders.DTOs;

/// <summary>
/// Summary DTO for order lists.
/// </summary>
public record OrderSummaryDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusDescription { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "MYR";
    public DateTime CreatedAt { get; init; }
    public DateTime? PlacedAt { get; init; }

    public static OrderSummaryDto FromOrder(Order order)
    {
        var total = order.GetTotal();

        return new OrderSummaryDto
        {
            Id = order.Id.Value,
            Status = order.Status.ToString(),
            StatusDescription = order.Status.GetDescription(),
            ItemCount = order.GetItemCount(),
            Total = total.Amount,
            Currency = total.Currency,
            CreatedAt = order.CreatedAt,
            PlacedAt = order.PlacedAt
        };
    }
}