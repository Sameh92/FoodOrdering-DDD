using FoodOrdering.Domain.Common;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.ValueObjects;

namespace FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

/// <summary>
/// Restaurant Aggregate Root.
/// </summary>
public class Restaurant : AggregateRoot<RestaurantId>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public OpeningHours OpeningHours { get; private set; } = null!;
    public Rating AverageRating { get; private set; } = Rating.Zero;
    public int TotalRatings { get; private set; }
    public int MaxConcurrentOrders { get; private set; }
    public int PendingOrderCount { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<MenuItem> _menuItems = new();
    public IReadOnlyList<MenuItem> MenuItems => _menuItems.AsReadOnly();

    // Factory method
    public static Restaurant Create(
        string name,
        string description,
        Address address,
        OpeningHours? openingHours = null,
        int maxConcurrentOrders = 20)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Restaurant name is required", nameof(name));

        return new Restaurant
        {
            Id = RestaurantId.New(),
            Name = name,
            Description = description ?? string.Empty,
            Address = address,
            OpeningHours = openingHours ?? OpeningHours.Default(),
            AverageRating = Rating.Zero,
            TotalRatings = 0,
            MaxConcurrentOrders = maxConcurrentOrders,
            PendingOrderCount = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Menu management
    public MenuItem AddMenuItem(
        string name,
        string description,
        Money price,
        string category,
        int preparationTimeMinutes = 15)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Menu item name is required", nameof(name));

        var menuItem = new MenuItem(
            MenuItemId.New(),
            name,
            description,
            price,
            category,
            preparationTimeMinutes
        );

        _menuItems.Add(menuItem);
        return menuItem;
    }

    public void UpdateMenuItem(
        MenuItemId menuItemId,
        string name,
        string description,
        Money price,
        string category)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId)
            ?? throw new InvalidOperationException("Menu item not found");

        menuItem.UpdateDetails(name, description, price, category);
    }

    public void SetMenuItemAvailability(MenuItemId menuItemId, bool isAvailable)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId)
            ?? throw new InvalidOperationException("Menu item not found");

        menuItem.SetAvailability(isAvailable);
    }

    public void RemoveMenuItem(MenuItemId menuItemId)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId)
            ?? throw new InvalidOperationException("Menu item not found");

        _menuItems.Remove(menuItem);
    }

    // Business operations
    public bool IsOpenAt(DateTime dateTime)
    {
        return IsActive && OpeningHours.IsOpenAt(dateTime);
    }

    public bool IsCurrentlyOpen()
    {
        return IsOpenAt(DateTime.UtcNow);
    }

    public bool CanAcceptOrders()
    {
        return IsActive && IsCurrentlyOpen() && PendingOrderCount < MaxConcurrentOrders;
    }

    public bool HasMenuItemAvailable(MenuItemId menuItemId)
    {
        var menuItem = _menuItems.FirstOrDefault(m => m.Id == menuItemId);
        return menuItem != null && menuItem.IsAvailable;
    }

    public MenuItem? GetMenuItem(MenuItemId menuItemId)
    {
        return _menuItems.FirstOrDefault(m => m.Id == menuItemId);
    }

    public TimeSpan EstimatePrepTime(int itemCount)
    {
        // Base time + additional time per item
        var baseMinutes = 10;
        var perItemMinutes = 3;
        var totalMinutes = baseMinutes + (itemCount * perItemMinutes);

        return TimeSpan.FromMinutes(Math.Min(totalMinutes, 60)); // Cap at 60 minutes
    }

    public void IncrementPendingOrders()
    {
        PendingOrderCount++;
    }

    public void DecrementPendingOrders()
    {
        if (PendingOrderCount > 0)
            PendingOrderCount--;
    }

    public void AddRating(decimal stars)
    {
        if (stars < 0 || stars > 5)
            throw new ArgumentException("Rating must be between 0 and 5", nameof(stars));

        var totalStars = (AverageRating.Stars * TotalRatings) + stars;
        TotalRatings++;
        AverageRating = new Rating(totalStars / TotalRatings);
    }

    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Restaurant name is required", nameof(name));

        Name = name;
        Description = description ?? string.Empty;
    }

    public void UpdateOpeningHours(OpeningHours openingHours)
    {
        OpeningHours = openingHours;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    // Private constructor for EF Core
    private Restaurant() { }
}