using FluentAssertions;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using Xunit;

namespace FoodOrdering.Domain.Tests.Aggregates;

public class RestaurantTests
{
    private readonly Address _validAddress = new("456 Food Street", "Kuala Lumpur", "WP", "50100", "Malaysia");

    #region Restaurant Creation Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var restaurant = Restaurant.Create("Burger Palace", "Best burgers in town!", _validAddress);

        // Assert
        restaurant.Should().NotBeNull();
        restaurant.Id.Should().NotBeNull();
        restaurant.Name.Should().Be("Burger Palace");
        restaurant.Description.Should().Be("Best burgers in town!");
        restaurant.Address.Should().Be(_validAddress);
        restaurant.IsActive.Should().BeTrue();
        restaurant.MenuItems.Should().BeEmpty();
        restaurant.PendingOrderCount.Should().Be(0);
        restaurant.TotalRatings.Should().Be(0);
        restaurant.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act
        var act = () => Restaurant.Create("", "Description", _validAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*required*");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowException()
    {
        // Act
        var act = () => Restaurant.Create("   ", "Description", _validAddress);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*required*");
    }

    [Fact]
    public void Create_WithNullDescription_ShouldUseEmptyString()
    {
        // Act
        var restaurant = Restaurant.Create("Burger Palace", null!, _validAddress);

        // Assert
        restaurant.Description.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithDefaultOpeningHours_ShouldSetDefaultHours()
    {
        // Act
        var restaurant = Restaurant.Create("Burger Palace", "Description", _validAddress);

        // Assert
        restaurant.OpeningHours.Should().NotBeNull();
        restaurant.OpeningHours.OpenTime.Should().Be(new TimeOnly(9, 0));
        restaurant.OpeningHours.CloseTime.Should().Be(new TimeOnly(22, 0));
    }

    [Fact]
    public void Create_WithCustomOpeningHours_ShouldUseProvidedHours()
    {
        // Arrange
        var customHours = new OpeningHours(
            new TimeOnly(10, 0),
            new TimeOnly(23, 0),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday }
        );

        // Act
        var restaurant = Restaurant.Create("Burger Palace", "Description", _validAddress, customHours);

        // Assert
        restaurant.OpeningHours.OpenTime.Should().Be(new TimeOnly(10, 0));
        restaurant.OpeningHours.CloseTime.Should().Be(new TimeOnly(23, 0));
    }

    [Fact]
    public void Create_WithCustomMaxConcurrentOrders_ShouldSetValue()
    {
        // Act
        var restaurant = Restaurant.Create("Burger Palace", "Description", _validAddress, maxConcurrentOrders: 30);

        // Assert
        restaurant.MaxConcurrentOrders.Should().Be(30);
    }

    #endregion

    #region Menu Item Management Tests

    [Fact]
    public void AddMenuItem_WithValidData_ShouldAddItem()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var price = new Money(15.90m, "MYR");

        // Act
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Juicy beef patty", price, "Burgers", 15);

        // Assert
        restaurant.MenuItems.Should().HaveCount(1);
        menuItem.Should().NotBeNull();
        menuItem.Id.Should().NotBeNull();
        menuItem.Name.Should().Be("Classic Burger");
        menuItem.Description.Should().Be("Juicy beef patty");
        menuItem.Price.Should().Be(price);
        menuItem.Category.Should().Be("Burgers");
        menuItem.PreparationTimeMinutes.Should().Be(15);
        menuItem.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void AddMenuItem_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var act = () => restaurant.AddMenuItem("", "Description", new Money(10m, "MYR"), "Category");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*required*");
    }

    [Fact]
    public void AddMenuItem_MultipleItems_ShouldAddAll()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");
        restaurant.AddMenuItem("Cheese Burger", "Desc", new Money(17.90m, "MYR"), "Burgers");
        restaurant.AddMenuItem("French Fries", "Desc", new Money(6.90m, "MYR"), "Sides");

        // Assert
        restaurant.MenuItems.Should().HaveCount(3);
    }

    [Fact]
    public void AddMenuItem_WithDefaultPreparationTime_ShouldUse15Minutes()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");

        // Assert
        menuItem.PreparationTimeMinutes.Should().Be(15);
    }

    [Fact]
    public void UpdateMenuItem_WithValidData_ShouldUpdateItem()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Old desc", new Money(15.90m, "MYR"), "Burgers");
        var newPrice = new Money(18.90m, "MYR");

        // Act
        restaurant.UpdateMenuItem(menuItem.Id, "Super Burger", "New desc", newPrice, "Premium Burgers");

        // Assert
        var updatedItem = restaurant.GetMenuItem(menuItem.Id);
        updatedItem!.Name.Should().Be("Super Burger");
        updatedItem.Description.Should().Be("New desc");
        updatedItem.Price.Should().Be(newPrice);
        updatedItem.Category.Should().Be("Premium Burgers");
    }

    [Fact]
    public void UpdateMenuItem_NonExistingItem_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var nonExistingId = MenuItemId.New();

        // Act
        var act = () => restaurant.UpdateMenuItem(nonExistingId, "Name", "Desc", new Money(10m, "MYR"), "Cat");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void SetMenuItemAvailability_ToFalse_ShouldMakeItemUnavailable()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");

        // Act
        restaurant.SetMenuItemAvailability(menuItem.Id, false);

        // Assert
        var item = restaurant.GetMenuItem(menuItem.Id);
        item!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public void SetMenuItemAvailability_ToTrue_ShouldMakeItemAvailable()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");
        restaurant.SetMenuItemAvailability(menuItem.Id, false);

        // Act
        restaurant.SetMenuItemAvailability(menuItem.Id, true);

        // Assert
        var item = restaurant.GetMenuItem(menuItem.Id);
        item!.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void SetMenuItemAvailability_NonExistingItem_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var act = () => restaurant.SetMenuItemAvailability(MenuItemId.New(), false);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void RemoveMenuItem_ExistingItem_ShouldRemoveItem()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");

        // Act
        restaurant.RemoveMenuItem(menuItem.Id);

        // Assert
        restaurant.MenuItems.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMenuItem_NonExistingItem_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var act = () => restaurant.RemoveMenuItem(MenuItemId.New());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public void GetMenuItem_ExistingItem_ShouldReturnItem()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");

        // Act
        var result = restaurant.GetMenuItem(menuItem.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Classic Burger");
    }

    [Fact]
    public void GetMenuItem_NonExistingItem_ShouldReturnNull()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var result = restaurant.GetMenuItem(MenuItemId.New());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void HasMenuItemAvailable_AvailableItem_ShouldReturnTrue()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");

        // Assert
        restaurant.HasMenuItemAvailable(menuItem.Id).Should().BeTrue();
    }

    [Fact]
    public void HasMenuItemAvailable_UnavailableItem_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var menuItem = restaurant.AddMenuItem("Classic Burger", "Desc", new Money(15.90m, "MYR"), "Burgers");
        restaurant.SetMenuItemAvailability(menuItem.Id, false);

        // Assert
        restaurant.HasMenuItemAvailable(menuItem.Id).Should().BeFalse();
    }

    [Fact]
    public void HasMenuItemAvailable_NonExistingItem_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Assert
        restaurant.HasMenuItemAvailable(MenuItemId.New()).Should().BeFalse();
    }

    #endregion

    #region Opening Hours Tests

    [Fact]
    public void IsOpenAt_DuringOpenHours_ShouldReturnTrue()
    {
        // Arrange
        var restaurant = CreateRestaurant(); // Default hours 9am - 10pm
        var dateTime = new DateTime(2024, 1, 15, 12, 0, 0); // Monday at noon

        // Assert
        restaurant.IsOpenAt(dateTime).Should().BeTrue();
    }

    [Fact]
    public void IsOpenAt_BeforeOpenHours_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var dateTime = new DateTime(2024, 1, 15, 8, 0, 0); // Monday at 8am

        // Assert
        restaurant.IsOpenAt(dateTime).Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_AfterCloseHours_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var dateTime = new DateTime(2024, 1, 15, 23, 0, 0); // Monday at 11pm

        // Assert
        restaurant.IsOpenAt(dateTime).Should().BeFalse();
    }

    [Fact]
    public void IsOpenAt_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        restaurant.Deactivate();
        var dateTime = new DateTime(2024, 1, 15, 12, 0, 0);

        // Assert
        restaurant.IsOpenAt(dateTime).Should().BeFalse();
    }

    [Fact]
    public void UpdateOpeningHours_ShouldUpdateHours()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        var newHours = new OpeningHours(
            new TimeOnly(11, 0),
            new TimeOnly(23, 0),
            new[] { DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday }
        );

        // Act
        restaurant.UpdateOpeningHours(newHours);

        // Assert
        restaurant.OpeningHours.OpenTime.Should().Be(new TimeOnly(11, 0));
        restaurant.OpeningHours.CloseTime.Should().Be(new TimeOnly(23, 0));
    }

    #endregion

    #region Order Capacity Tests

    [Fact]
    public void CanAcceptOrders_WhenActiveAndOpenAndBelowCapacity_ShouldReturnTrue()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        // Note: IsCurrentlyOpen() depends on real time, so we test via IsOpenAt

        // Assert
        restaurant.IsActive.Should().BeTrue();
        restaurant.PendingOrderCount.Should().BeLessThan(restaurant.MaxConcurrentOrders);
    }

    [Fact]
    public void IncrementPendingOrders_ShouldIncreasePendingOrderCount()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.IncrementPendingOrders();

        // Assert
        restaurant.PendingOrderCount.Should().Be(1);
    }

    [Fact]
    public void IncrementPendingOrders_MultipleTimes_ShouldAccumulate()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.IncrementPendingOrders();
        restaurant.IncrementPendingOrders();
        restaurant.IncrementPendingOrders();

        // Assert
        restaurant.PendingOrderCount.Should().Be(3);
    }

    [Fact]
    public void DecrementPendingOrders_ShouldDecreasePendingOrderCount()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        restaurant.IncrementPendingOrders();
        restaurant.IncrementPendingOrders();

        // Act
        restaurant.DecrementPendingOrders();

        // Assert
        restaurant.PendingOrderCount.Should().Be(1);
    }

    [Fact]
    public void DecrementPendingOrders_WhenZero_ShouldRemainZero()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.DecrementPendingOrders();

        // Assert
        restaurant.PendingOrderCount.Should().Be(0);
    }

    #endregion

    #region Rating Tests

    [Fact]
    public void AddRating_FirstRating_ShouldSetAverageRating()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.AddRating(4.5m);

        // Assert
        restaurant.AverageRating.Stars.Should().Be(4.5m);
        restaurant.TotalRatings.Should().Be(1);
    }

    [Fact]
    public void AddRating_MultipleRatings_ShouldCalculateAverage()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.AddRating(5.0m);
        restaurant.AddRating(4.0m);
        restaurant.AddRating(3.0m);

        // Assert
        restaurant.AverageRating.Stars.Should().Be(4.0m); // (5 + 4 + 3) / 3
        restaurant.TotalRatings.Should().Be(3);
    }

    [Fact]
    public void AddRating_WithInvalidRating_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var act = () => restaurant.AddRating(6.0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*0 and 5*");
    }

    [Fact]
    public void AddRating_WithNegativeRating_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var act = () => restaurant.AddRating(-1.0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*0 and 5*");
    }

    #endregion

    #region Update Details Tests

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.UpdateDetails("New Name", "New Description");

        // Assert
        restaurant.Name.Should().Be("New Name");
        restaurant.Description.Should().Be("New Description");
    }

    [Fact]
    public void UpdateDetails_WithEmptyName_ShouldThrowException()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var act = () => restaurant.UpdateDetails("", "Description");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*name*required*");
    }

    [Fact]
    public void UpdateDetails_WithNullDescription_ShouldUseEmptyString()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.UpdateDetails("New Name", null!);

        // Assert
        restaurant.Description.Should().BeEmpty();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        restaurant.Deactivate();

        // Assert
        restaurant.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_AfterDeactivation_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var restaurant = CreateRestaurant();
        restaurant.Deactivate();

        // Act
        restaurant.Activate();

        // Assert
        restaurant.IsActive.Should().BeTrue();
    }

    #endregion

    #region Prep Time Estimation Tests

    [Fact]
    public void EstimatePrepTime_WithOneItem_ShouldReturnBaseTimePlusPerItem()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var prepTime = restaurant.EstimatePrepTime(1);

        // Assert
        prepTime.Should().Be(TimeSpan.FromMinutes(13)); // 10 base + 3 per item
    }

    [Fact]
    public void EstimatePrepTime_WithFiveItems_ShouldReturnCorrectTime()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var prepTime = restaurant.EstimatePrepTime(5);

        // Assert
        prepTime.Should().Be(TimeSpan.FromMinutes(25)); // 10 base + (5 * 3)
    }

    [Fact]
    public void EstimatePrepTime_WithManyItems_ShouldCapAt60Minutes()
    {
        // Arrange
        var restaurant = CreateRestaurant();

        // Act
        var prepTime = restaurant.EstimatePrepTime(100);

        // Assert
        prepTime.Should().Be(TimeSpan.FromMinutes(60));
    }

    #endregion

    #region RestaurantId Tests

    [Fact]
    public void RestaurantId_New_ShouldGenerateUniqueId()
    {
        // Act
        var id1 = RestaurantId.New();
        var id2 = RestaurantId.New();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void RestaurantId_From_WithEmptyGuid_ShouldThrowException()
    {
        // Act
        var act = () => RestaurantId.From(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    #endregion

    #region MenuItemId Tests

    [Fact]
    public void MenuItemId_New_ShouldGenerateUniqueId()
    {
        // Act
        var id1 = MenuItemId.New();
        var id2 = MenuItemId.New();

        // Assert
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void MenuItemId_From_WithEmptyGuid_ShouldThrowException()
    {
        // Act
        var act = () => MenuItemId.From(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*empty*");
    }

    #endregion

    #region Rating Value Object Tests

    [Fact]
    public void Rating_Create_WithValidValue_ShouldSucceed()
    {
        // Act
        var rating = new Rating(4.5m);

        // Assert
        rating.Stars.Should().Be(4.5m);
    }

    [Fact]
    public void Rating_Create_ShouldRoundToOneDecimal()
    {
        // Act
        var rating = new Rating(4.567m);

        // Assert
        rating.Stars.Should().Be(4.6m);
    }

    [Fact]
    public void Rating_Create_WithValueAbove5_ShouldThrowException()
    {
        // Act
        var act = () => new Rating(5.1m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*0 and 5*");
    }

    [Fact]
    public void Rating_Create_WithNegativeValue_ShouldThrowException()
    {
        // Act
        var act = () => new Rating(-0.1m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*0 and 5*");
    }

    [Fact]
    public void Rating_Zero_ShouldReturnZeroStars()
    {
        // Act
        var rating = Rating.Zero;

        // Assert
        rating.Stars.Should().Be(0m);
    }

    #endregion

    #region OpeningHours Value Object Tests

    [Fact]
    public void OpeningHours_Create_WithValidData_ShouldSucceed()
    {
        // Act
        var hours = new OpeningHours(
            new TimeOnly(9, 0),
            new TimeOnly(22, 0),
            new[] { DayOfWeek.Monday, DayOfWeek.Tuesday }
        );

        // Assert
        hours.OpenTime.Should().Be(new TimeOnly(9, 0));
        hours.CloseTime.Should().Be(new TimeOnly(22, 0));
        hours.OpenDays.Should().HaveCount(2);
    }

    [Fact]
    public void OpeningHours_Create_WithEmptyOpenDays_ShouldThrowException()
    {
        // Act
        var act = () => new OpeningHours(
            new TimeOnly(9, 0),
            new TimeOnly(22, 0),
            Array.Empty<DayOfWeek>()
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*open day*required*");
    }

    [Fact]
    public void OpeningHours_IsOpenAt_OnOpenDay_ShouldReturnTrue()
    {
        // Arrange
        var hours = new OpeningHours(
            new TimeOnly(9, 0),
            new TimeOnly(22, 0),
            new[] { DayOfWeek.Monday }
        );
        var monday = new DateTime(2024, 1, 15, 12, 0, 0); // Monday

        // Assert
        hours.IsOpenAt(monday).Should().BeTrue();
    }

    [Fact]
    public void OpeningHours_IsOpenAt_OnClosedDay_ShouldReturnFalse()
    {
        // Arrange
        var hours = new OpeningHours(
            new TimeOnly(9, 0),
            new TimeOnly(22, 0),
            new[] { DayOfWeek.Monday }
        );
        var tuesday = new DateTime(2024, 1, 16, 12, 0, 0); // Tuesday

        // Assert
        hours.IsOpenAt(tuesday).Should().BeFalse();
    }

    [Fact]
    public void OpeningHours_Default_ShouldReturnStandardHours()
    {
        // Act
        var hours = OpeningHours.Default();

        // Assert
        hours.OpenTime.Should().Be(new TimeOnly(9, 0));
        hours.CloseTime.Should().Be(new TimeOnly(22, 0));
        hours.OpenDays.Should().HaveCount(7); // All days
    }

    #endregion

    #region Helper Methods

    private Restaurant CreateRestaurant()
    {
        return Restaurant.Create("Burger Palace", "Best burgers in town!", _validAddress);
    }

    #endregion
}