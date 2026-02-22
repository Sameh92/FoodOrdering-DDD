namespace FoodOrdering.Domain.Restaurants.ValueObjects;

/// <summary>
/// Rating Value Object (1-5 stars).
/// </summary>
public record Rating
{
    public decimal Stars { get; }

    public Rating(decimal stars)
    {
        if (stars < 0 || stars > 5)
            throw new ArgumentException("Rating must be between 0 and 5", nameof(stars));

        Stars = Math.Round(stars, 1);
    }

    public static Rating Zero => new(0);

    public override string ToString() => $"{Stars:F1} stars";
}
