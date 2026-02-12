using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.Domain.Ordering.Services;

/// <summary>
/// Domain Service for calculating delivery fees.
/// 
/// This is a Domain Service because:
/// - The logic involves multiple Aggregates (Customer, Restaurant locations)
/// - It doesn't belong to any single Aggregate
/// - It contains business rules about pricing
/// - It's stateless
/// </summary>
public class DeliveryFeeCalculator
{
    private const decimal BaseRatePerKm = 0.50m;
    private const decimal MinimumFee = 3.00m;
    private const decimal MaximumFee = 15.00m;
    private const decimal PeakHourMultiplier = 1.5m;
    private const decimal PremiumMemberDiscount = 0.20m; // 20% off
    private const decimal GoldMemberDiscount = 0.10m;    // 10% off
    private const decimal SilverMemberDiscount = 0.05m;  // 5% off

    /// <summary>
    /// Calculate delivery fee based on distance, time, and customer status.
    /// </summary>
    public Money CalculateFee(
        Address restaurantAddress,
        Address customerAddress,
        MembershipLevel membershipLevel,
        DateTime deliveryTime,
        string currency = "MYR")
    {
        // Calculate distance (simplified)
        var distanceKm = CalculateDistance(restaurantAddress, customerAddress);

        // Base fee calculation
        var baseFee = (decimal)distanceKm * BaseRatePerKm;

        // Apply minimum fee
        baseFee = Math.Max(baseFee, MinimumFee);

        // Apply peak hour surcharge
        if (IsPeakHour(deliveryTime))
            baseFee *= PeakHourMultiplier;

        // Apply membership discount
        var discount = GetMembershipDiscount(membershipLevel);
        baseFee *= (1 - discount);

        // Apply maximum fee cap
        baseFee = Math.Min(baseFee, MaximumFee);

        // Round to 2 decimal places
        baseFee = Math.Round(baseFee, 2);

        return new Money(baseFee, currency);
    }

    /// <summary>
    /// Check if delivery is available between two addresses.
    /// </summary>
    public bool IsDeliveryAvailable(Address restaurantAddress, Address customerAddress)
    {
        var distanceKm = CalculateDistance(restaurantAddress, customerAddress);
        return distanceKm <= 15; // Max 15km delivery radius
    }

    /// <summary>
    /// Get estimated delivery time.
    /// </summary>
    public TimeSpan EstimateDeliveryTime(Address restaurantAddress, Address customerAddress)
    {
        var distanceKm = CalculateDistance(restaurantAddress, customerAddress);

        // Assume average speed of 25 km/h in city traffic
        var travelMinutes = (distanceKm / 25.0) * 60;

        // Add buffer time
        travelMinutes += 5;

        return TimeSpan.FromMinutes(Math.Ceiling(travelMinutes));
    }

    private double CalculateDistance(Address from, Address to)
    {
        // Simplified distance calculation
        // In real application, would use Google Maps API or similar

        if (from.PostalCode == to.PostalCode)
            return 1.5;

        if (from.City.Equals(to.City, StringComparison.OrdinalIgnoreCase))
            return 5.0;

        if (from.State.Equals(to.State, StringComparison.OrdinalIgnoreCase))
            return 10.0;

        return 20.0; // Different state/region
    }

    private bool IsPeakHour(DateTime time)
    {
        var hour = time.Hour;

        // Lunch peak: 11am - 2pm
        if (hour >= 11 && hour <= 14)
            return true;

        // Dinner peak: 6pm - 9pm
        if (hour >= 18 && hour <= 21)
            return true;

        return false;
    }

    private decimal GetMembershipDiscount(MembershipLevel level)
    {
        return level switch
        {
            MembershipLevel.Premium => PremiumMemberDiscount,
            MembershipLevel.Gold => GoldMemberDiscount,
            MembershipLevel.Silver => SilverMemberDiscount,
            _ => 0m
        };
    }
}

/// <summary>
/// Domain Service for checking restaurant availability.
/// </summary>
public class RestaurantAvailabilityService
{
    /// <summary>
    /// Check if restaurant can accept a new order.
    /// </summary>
    public bool CanAcceptOrder(Restaurant restaurant, DateTime orderTime)
    {
        if (!restaurant.IsActive)
            return false;

        if (!restaurant.IsOpenAt(orderTime))
            return false;

        if (!restaurant.CanAcceptOrders())
            return false;

        return true;
    }

    /// <summary>
    /// Get estimated time until restaurant can accept orders.
    /// </summary>
    public TimeSpan? GetWaitTime(Restaurant restaurant, DateTime currentTime)
    {
        if (!restaurant.IsActive)
            return null;

        if (restaurant.CanAcceptOrders())
            return TimeSpan.Zero;

        // If at capacity, estimate wait time based on average order time
        var averageOrderTime = TimeSpan.FromMinutes(20);
        return averageOrderTime;
    }
}

/// <summary>
/// Domain Service for estimating order delivery time.
/// </summary>
public class DeliveryTimeEstimator
{
    private readonly DeliveryFeeCalculator _feeCalculator;

    public DeliveryTimeEstimator(DeliveryFeeCalculator feeCalculator)
    {
        _feeCalculator = feeCalculator;
    }

    /// <summary>
    /// Estimate total delivery time from order placement to delivery.
    /// </summary>
    public DateTime EstimateDeliveryTime(
        Restaurant restaurant,
        Address customerAddress,
        int itemCount,
        DateTime orderTime)
    {
        // Restaurant prep time
        var prepTime = restaurant.EstimatePrepTime(itemCount);

        // Delivery time
        var deliveryTime = _feeCalculator.EstimateDeliveryTime(
            restaurant.Address,
            customerAddress
        );

        // Add buffer for pickup
        var pickupBuffer = TimeSpan.FromMinutes(5);

        var totalTime = prepTime + pickupBuffer + deliveryTime;

        return orderTime.Add(totalTime);
    }
}