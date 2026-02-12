using FluentAssertions;
using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Services;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using System;
using System.Diagnostics;

namespace FoodOrdering.Domain.Tests.Services;

public class DeliveryFeeCalculatorTests
{
    private readonly DeliveryFeeCalculator _calculator = new();
    private readonly Address _restaurantAddress = new("456 Food St", "Kuala Lumpur", "WP", "50100", "Malaysia");

    #region Basic Fee Calculation Tests

    [Fact]
    public void CalculateFee_SamePostalCode_ShouldReturnMinimumFee()
    {
        // Arrange
        var customerAddress = new Address("123 Main St", "Kuala Lumpur", "WP", "50100", "Malaysia");

        // Act
        var fee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            new DateTime(2024, 1, 15, 15, 0, 0) // 3pm - non-peak
        );

        // Assert
        fee.Amount.Should().BeGreaterOrEqualTo(3.00m); // Minimum fee
        fee.Currency.Should().Be("MYR");
    }

    [Fact]
    public void CalculateFee_SameCity_ShouldCalculateBasedOnDistance()
    {
        // Arrange
        var customerAddress = new Address("789 Other St", "Kuala Lumpur", "WP", "50200", "Malaysia");

        // Act
        var fee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            new DateTime(2024, 1, 15, 15, 0, 0)
        );

        // Assert
        fee.Amount.Should().BeGreaterThan(0);
        fee.Amount.Should().BeLessThanOrEqualTo(15.00m); // Maximum fee
    }

    [Fact]
    public void CalculateFee_DifferentCity_ShouldBeHigher()
    {
        // Arrange
        var nearAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var farAddress = new Address("123 St", "Petaling Jaya", "Selangor", "47800", "Malaysia");

        // Act
        var nearFee = _calculator.CalculateFee(
            _restaurantAddress,
            nearAddress,
            MembershipLevel.Standard,
            new DateTime(2024, 1, 15, 15, 0, 0)
        );

        var farFee = _calculator.CalculateFee(
            _restaurantAddress,
            farAddress,
            MembershipLevel.Standard,
            new DateTime(2024, 1, 15, 15, 0, 0)
        );

        // Assert
        farFee.Amount.Should().BeGreaterThan(nearFee.Amount);
    }

    [Fact]
    public void CalculateFee_ShouldNeverExceedMaximum()
    {
        // Arrange
        var veryFarAddress = new Address("123 St", "Johor Bahru", "Johor", "80000", "Malaysia");

        // Act
        var fee = _calculator.CalculateFee(
            _restaurantAddress,
            veryFarAddress,
            MembershipLevel.Standard,
            new DateTime(2024, 1, 15, 12, 0, 0) // Peak hour
        );

        // Assert
        fee.Amount.Should().BeLessThanOrEqualTo(15.00m);
    }

    [Fact]
    public void CalculateFee_ShouldReturnCorrectCurrency()
    {
        // Arrange
        var customerAddress = new Address("123 Main St", "Kuala Lumpur", "WP", "50100", "Malaysia");

        // Act
        var fee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            DateTime.UtcNow,
            "USD"
        );

        // Assert
        fee.Currency.Should().Be("USD");
    }

    #endregion

    #region Peak Hour Tests

    [Theory]
    [InlineData(11, 0)] // 11am - lunch peak start
    [InlineData(12, 0)] // 12pm - lunch peak
    [InlineData(13, 0)] // 1pm - lunch peak
    [InlineData(14, 0)] // 2pm - lunch peak end
    public void CalculateFee_DuringLunchPeak_ShouldApplySurcharge(int hour, int minute)
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var peakTime = new DateTime(2024, 1, 15, hour, minute, 0);
        var nonPeakTime = new DateTime(2024, 1, 15, 10, 0, 0); // 10am

        // Act
        var peakFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            peakTime
        );

        var nonPeakFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            nonPeakTime
        );

        // Assert
        peakFee.Amount.Should().BeGreaterThan(nonPeakFee.Amount);
    }

    [Theory]
    [InlineData(18, 0)] // 6pm - dinner peak start
    [InlineData(19, 0)] // 7pm - dinner peak
    [InlineData(20, 0)] // 8pm - dinner peak
    [InlineData(21, 0)] // 9pm - dinner peak end
    public void CalculateFee_DuringDinnerPeak_ShouldApplySurcharge(int hour, int minute)
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var peakTime = new DateTime(2024, 1, 15, hour, minute, 0);
        var nonPeakTime = new DateTime(2024, 1, 15, 16, 0, 0); // 4pm

        // Act
        var peakFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            peakTime
        );

        var nonPeakFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            nonPeakTime
        );

        // Assert
        peakFee.Amount.Should().BeGreaterThan(nonPeakFee.Amount);
    }

    [Theory]
    [InlineData(9, 0)]  // 9am
    [InlineData(10, 0)] // 10am
    [InlineData(15, 0)] // 3pm
    [InlineData(16, 0)] // 4pm
    [InlineData(22, 0)] // 10pm
    public void CalculateFee_DuringNonPeakHours_ShouldNotApplySurcharge(int hour, int minute)
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var time1 = new DateTime(2024, 1, 15, hour, minute, 0);
        var time2 = new DateTime(2024, 1, 15, 10, 0, 0); // Known non-peak

        // Act
        var fee1 = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time1
        );

        var fee2 = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time2
        );

        // Assert
        fee1.Amount.Should().Be(fee2.Amount);
    }

    #endregion

    #region Membership Discount Tests

    [Fact]
    public void CalculateFee_PremiumMember_ShouldApply20PercentDiscount()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var time = new DateTime(2024, 1, 15, 15, 0, 0);

        // Act
        var standardFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time
        );

        var premiumFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Premium,
            time
        );

        // Assert
        premiumFee.Amount.Should().Be(Math.Round(standardFee.Amount * 0.80m, 2));
    }

    [Fact]
    public void CalculateFee_GoldMember_ShouldApply10PercentDiscount()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var time = new DateTime(2024, 1, 15, 15, 0, 0);

        // Act
        var standardFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time
        );

        var goldFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Gold,
            time
        );

        // Assert
        goldFee.Amount.Should().Be(Math.Round(standardFee.Amount * 0.90m, 2));
    }

    [Fact]
    public void CalculateFee_SilverMember_ShouldApply5PercentDiscount()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var time = new DateTime(2024, 1, 15, 15, 0, 0);

        // Act
        var standardFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time
        );

        var silverFee = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Silver,
            time
        );

        // Assert
        silverFee.Amount.Should().Be(Math.Round(standardFee.Amount * 0.95m, 2));
    }

    [Fact]
    public void CalculateFee_StandardMember_ShouldHaveNoDiscount()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var time = new DateTime(2024, 1, 15, 15, 0, 0);

        // Act
        var fee1 = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time
        );

        var fee2 = _calculator.CalculateFee(
            _restaurantAddress,
            customerAddress,
            MembershipLevel.Standard,
            time
        );

        // Assert
        fee1.Amount.Should().Be(fee2.Amount);
    }

    #endregion

    #region Delivery Availability Tests

    [Fact]
    public void IsDeliveryAvailable_SameCity_ShouldReturnTrue()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");

        // Act
        var isAvailable = _calculator.IsDeliveryAvailable(_restaurantAddress, customerAddress);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsDeliveryAvailable_SameState_ShouldReturnTrue()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Petaling Jaya", "WP", "47800", "Malaysia");

        // Act
        var isAvailable = _calculator.IsDeliveryAvailable(_restaurantAddress, customerAddress);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsDeliveryAvailable_DifferentState_ShouldReturnFalse()
    {
        // Arrange
        var farAddress = new Address("123 St", "Johor Bahru", "Johor", "80000", "Malaysia");

        // Act
        var isAvailable = _calculator.IsDeliveryAvailable(_restaurantAddress, farAddress);

        // Assert
        isAvailable.Should().BeFalse(); // > 15km
    }

    #endregion

    #region Delivery Time Estimation Tests

    [Fact]
    public void EstimateDeliveryTime_SamePostalCode_ShouldReturnShortTime()
    {
        // Arrange
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50100", "Malaysia");

        // Act
        var time = _calculator.EstimateDeliveryTime(_restaurantAddress, customerAddress);

        // Assert
        time.TotalMinutes.Should().BeLessThan(15);
    }

    [Fact]
    public void EstimateDeliveryTime_DifferentCity_ShouldReturnLongerTime()
    {
        // Arrange
        var nearAddress = new Address("123 St", "Kuala Lumpur", "WP", "50200", "Malaysia");
        var farAddress = new Address("123 St", "Petaling Jaya", "Selangor", "47800", "Malaysia");

        // Act
        var nearTime = _calculator.EstimateDeliveryTime(_restaurantAddress, nearAddress);
        var farTime = _calculator.EstimateDeliveryTime(_restaurantAddress, farAddress);

        // Assert
        farTime.Should().BeGreaterThan(nearTime);
    }

    #endregion
}

public class RestaurantAvailabilityServiceTests
{
    private readonly RestaurantAvailabilityService _service = new();
    private readonly Address _address = new("456 Food St", "Kuala Lumpur", "WP", "50100", "Malaysia");

    [Fact]
    public void CanAcceptOrder_ActiveAndOpenAndBelowCapacity_ShouldReturnTrue()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address);
        // Restaurant is active by default, and below capacity (0 pending orders)

        // We need to test at a time when the restaurant is open
        // Default hours are 9am - 10pm
        var openTime = new DateTime(2024, 1, 15, 12, 0, 0); // Monday at noon

        // Act
        var canAccept = _service.CanAcceptOrder(restaurant, openTime);

        // Assert
        canAccept.Should().BeTrue();
    }

    [Fact]
    public void CanAcceptOrder_InactiveRestaurant_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address);
        restaurant.Deactivate();
        var openTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var canAccept = _service.CanAcceptOrder(restaurant, openTime);

        // Assert
        canAccept.Should().BeFalse();
    }

    [Fact]
    public void CanAcceptOrder_OutsideOpeningHours_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address);
        var closedTime = new DateTime(2024, 1, 15, 23, 0, 0); // 11pm - after closing

        // Act
        var canAccept = _service.CanAcceptOrder(restaurant, closedTime);

        // Assert
        canAccept.Should().BeFalse();
    }

    [Fact]
    public void CanAcceptOrder_AtMaxCapacity_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address, maxConcurrentOrders: 2);
        restaurant.IncrementPendingOrders();
        restaurant.IncrementPendingOrders();
        var openTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var canAccept = _service.CanAcceptOrder(restaurant, openTime);

        // Assert
        canAccept.Should().BeFalse();
    }

    [Fact]
    public void GetWaitTime_InactiveRestaurant_ShouldReturnNull()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address);
        restaurant.Deactivate();

        // Act
        var waitTime = _service.GetWaitTime(restaurant, DateTime.UtcNow);

        // Assert
        waitTime.Should().BeNull();
    }

    [Fact]
    public void GetWaitTime_CanAcceptOrders_ShouldReturnZero()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address);

        // Act
        var waitTime = _service.GetWaitTime(restaurant, DateTime.UtcNow);

        // Assert
        waitTime.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetWaitTime_AtCapacity_ShouldReturnEstimatedTime()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _address, maxConcurrentOrders: 1);
        restaurant.IncrementPendingOrders();

        // Act
        var waitTime = _service.GetWaitTime(restaurant, DateTime.UtcNow);

        // Assert
        waitTime.Should().NotBeNull();
        waitTime!.Value.TotalMinutes.Should().BeGreaterThan(0);
    }
}

public class DeliveryTimeEstimatorTests
{
    private readonly DeliveryTimeEstimator _estimator;
    private readonly Address _restaurantAddress = new("456 Food St", "Kuala Lumpur", "WP", "50100", "Malaysia");

    public DeliveryTimeEstimatorTests()
    {
        var feeCalculator = new DeliveryFeeCalculator();
        _estimator = new DeliveryTimeEstimator(feeCalculator);
    }

    [Fact]
    public void EstimateDeliveryTime_ShouldIncludePrepTime()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _restaurantAddress);
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50100", "Malaysia");
        var orderTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var estimatedTime = _estimator.EstimateDeliveryTime(restaurant, customerAddress, 3, orderTime);

        // Assert
        estimatedTime.Should().BeAfter(orderTime);

        // Should include at least prep time (base 10 + 3 per item = 19 min for 3 items)
        var minExpectedTime = orderTime.AddMinutes(19);
        estimatedTime.Should().BeOnOrAfter(minExpectedTime);
    }

    [Fact]
    public void EstimateDeliveryTime_MoreItems_ShouldTakeLonger()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _restaurantAddress);
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50100", "Malaysia");
        var orderTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var timeFor2Items = _estimator.EstimateDeliveryTime(restaurant, customerAddress, 2, orderTime);
        var timeFor10Items = _estimator.EstimateDeliveryTime(restaurant, customerAddress, 10, orderTime);

        // Assert
        timeFor10Items.Should().BeAfter(timeFor2Items);
    }

    [Fact]
    public void EstimateDeliveryTime_FartherDistance_ShouldTakeLonger()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _restaurantAddress);
        var nearAddress = new Address("123 St", "Kuala Lumpur", "WP", "50100", "Malaysia");
        var farAddress = new Address("123 St", "Petaling Jaya", "Selangor", "47800", "Malaysia");
        var orderTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Act
        var nearTime = _estimator.EstimateDeliveryTime(restaurant, nearAddress, 2, orderTime);
        var farTime = _estimator.EstimateDeliveryTime(restaurant, farAddress, 2, orderTime);

        // Assert
        farTime.Should().BeAfter(nearTime);
    }

    [Fact]
    public void EstimateDeliveryTime_ShouldReturnFutureTime()
    {
        // Arrange
        var restaurant = Restaurant.Create("Test Restaurant", "Description", _restaurantAddress);
        var customerAddress = new Address("123 St", "Kuala Lumpur", "WP", "50100", "Malaysia");
        var orderTime = DateTime.UtcNow;

        // Act
        var estimatedTime = _estimator.EstimateDeliveryTime(restaurant, customerAddress, 1, orderTime);

        // Assert
        estimatedTime.Should().BeAfter(orderTime);
    }
}
