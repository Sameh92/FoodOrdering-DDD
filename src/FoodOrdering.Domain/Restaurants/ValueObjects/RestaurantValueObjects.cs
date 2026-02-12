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

/// <summary>
/// Opening Hours Value Object.
/// </summary>
public record OpeningHours
{
    public TimeOnly OpenTime { get; }
    public TimeOnly CloseTime { get; }
    public DayOfWeek[] OpenDays { get; }

    public OpeningHours(TimeOnly openTime, TimeOnly closeTime, DayOfWeek[] openDays)
    {
        if (openDays == null || openDays.Length == 0)
            throw new ArgumentException("At least one open day is required", nameof(openDays));

        OpenTime = openTime;
        CloseTime = closeTime;
        OpenDays = openDays;
    }

    public bool IsOpenAt(DateTime dateTime)
    {
        if (!OpenDays.Contains(dateTime.DayOfWeek))
            return false;

        var time = TimeOnly.FromDateTime(dateTime);

        // Handle overnight hours (e.g., 10pm - 2am)
        if (CloseTime < OpenTime)
        {
            return time >= OpenTime || time <= CloseTime;
        }

        return time >= OpenTime && time <= CloseTime;
    }

    public static OpeningHours Default()
    {
        return new OpeningHours(
            new TimeOnly(9, 0),
            new TimeOnly(22, 0),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday }
        );
    }
}
