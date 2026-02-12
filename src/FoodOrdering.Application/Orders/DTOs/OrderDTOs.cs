using FoodOrdering.Domain.Ordering.Aggregates.Order;

namespace FoodOrdering.Application.Orders.DTOs;

/// <summary>
/// Data Transfer Object for Order.
/// Used to transfer order data to the presentation layer.
/// </summary>
public record OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public Guid RestaurantId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StatusDescription { get; init; } = string.Empty;
    public AddressDto DeliveryAddress { get; init; } = null!;
    public List<OrderItemDto> Items { get; init; } = new();
    public decimal Subtotal { get; init; }
    public decimal? DeliveryFee { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = "MYR";
    public string? SpecialInstructions { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PlacedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? EstimatedReadyTime { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public string? CancellationReason { get; init; }

    public static OrderDto FromOrder(Order order)
    {
        var subtotal = order.GetSubtotal();
        var total = order.GetTotal();

        return new OrderDto
        {
            Id = order.Id.Value,
            CustomerId = order.CustomerId.Value,
            RestaurantId = order.RestaurantId.Value,
            Status = order.Status.ToString(),
            StatusDescription = order.Status.GetDescription(),
            DeliveryAddress = AddressDto.FromAddress(order.DeliveryAddress),
            Items = order.Items.Select(OrderItemDto.FromOrderItem).ToList(),
            Subtotal = subtotal.Amount,
            DeliveryFee = order.DeliveryFee?.Amount,
            Total = total.Amount,
            Currency = subtotal.Currency,
            SpecialInstructions = order.SpecialInstructions,
            CreatedAt = order.CreatedAt,
            PlacedAt = order.PlacedAt,
            ConfirmedAt = order.ConfirmedAt,
            EstimatedReadyTime = order.EstimatedReadyTime,
            DeliveredAt = order.DeliveredAt,
            CancellationReason = order.CancellationReason
        };
    }
}

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

/// <summary>
/// Data Transfer Object for Address.
/// </summary>
public record AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;

    public static AddressDto FromAddress(Domain.Ordering.ValueObjects.Address address)
    {
        return new AddressDto
        {
            Street = address.Street,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            Country = address.Country
        };
    }
}

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