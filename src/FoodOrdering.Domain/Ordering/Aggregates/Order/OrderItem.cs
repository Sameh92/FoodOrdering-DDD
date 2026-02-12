using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.Domain.Ordering.Aggregates.Order;

/// <summary>
/// OrderItem - Entity inside the Order Aggregate.
/// 
/// Key points:
/// - Lives INSIDE the Order aggregate boundary
/// - Cannot be accessed directly from outside
/// - All modifications go through the Order (Aggregate Root)
/// - Uses 'internal' to prevent outside access
/// </summary>
public class OrderItem
{
    public Guid Id { get; private set; }
    public MenuItemId MenuItemId { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public Money Price { get; private set; } = null!;
    public int Quantity { get; private set; }
    public string? SpecialInstructions { get; private set; }

    /// <summary>
    /// Internal constructor - only Order can create OrderItems.
    /// </summary>
    internal OrderItem(
        MenuItemId menuItemId,
        string name,
        Money price,
        int quantity,
        string? specialInstructions = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Id = Guid.NewGuid();
        MenuItemId = menuItemId;
        Name = name;
        Price = price;
        Quantity = quantity;
        SpecialInstructions = specialInstructions;
    }

    /// <summary>
    /// Internal - only Order can modify quantity.
    /// </summary>
    internal void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
    }

    /// <summary>
    /// Internal - only Order can increase quantity.
    /// </summary>
    internal void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Quantity += amount;
    }

    /// <summary>
    /// Internal - only Order can update special instructions.
    /// </summary>
    internal void SetSpecialInstructions(string? instructions)
    {
        SpecialInstructions = instructions;
    }

    /// <summary>
    /// Calculate total price for this item (price × quantity).
    /// </summary>
    public Money GetTotal() => Price * Quantity;

    // Private parameterless constructor for EF Core
    private OrderItem() { }
}