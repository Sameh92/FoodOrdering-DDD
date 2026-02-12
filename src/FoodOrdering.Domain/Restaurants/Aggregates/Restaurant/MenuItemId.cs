namespace FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

/// <summary>
/// Strongly typed ID for MenuItem.
/// </summary>
public record MenuItemId
{
    public Guid Value { get; }

    public MenuItemId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MenuItemId cannot be empty", nameof(value));

        Value = value;
    }

    public static MenuItemId New() => new(Guid.NewGuid());

    public static MenuItemId From(Guid value) => new(value);

    public static MenuItemId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MenuItemId menuItemId) => menuItemId.Value;
}
