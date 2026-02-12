namespace FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

/// <summary>
/// Strongly typed ID for Restaurant.
/// </summary>
public record RestaurantId
{
    public Guid Value { get; }

    public RestaurantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("RestaurantId cannot be empty", nameof(value));

        Value = value;
    }

    public static RestaurantId New() => new(Guid.NewGuid());

    public static RestaurantId From(Guid value) => new(value);

    public static RestaurantId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(RestaurantId restaurantId) => restaurantId.Value;
}