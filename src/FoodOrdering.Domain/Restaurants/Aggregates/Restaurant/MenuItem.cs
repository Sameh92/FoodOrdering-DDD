using FoodOrdering.Domain.Ordering.ValueObjects;

namespace FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

/// <summary>
/// Menu Item - Entity inside Restaurant Aggregate.
/// </summary>
public class MenuItem
{
    public MenuItemId Id { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = null!;
    public string Category { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; }
    public int PreparationTimeMinutes { get; private set; }

    internal MenuItem(
        MenuItemId id,
        string name,
        string description,
        Money price,
        string category,
        int preparationTimeMinutes)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Category = category;
        IsAvailable = true;
        PreparationTimeMinutes = preparationTimeMinutes;
    }

    internal void UpdateDetails(string name, string description, Money price, string category)
    {
        Name = name;
        Description = description;
        Price = price;
        Category = category;
    }

    internal void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    internal void UpdatePreparationTime(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Preparation time cannot be negative", nameof(minutes));

        PreparationTimeMinutes = minutes;
    }

    // Private constructor for EF Core
    private MenuItem() { }
}