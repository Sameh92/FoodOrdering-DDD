using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FoodOrdering.API.Controllers;
using FoodOrdering.API.DTOs.Common;
using FoodOrdering.API.DTOs.CustomerDTOs;
using FoodOrdering.API.DTOs.RestaurantDTOs;
using FoodOrdering.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace FoodOrdering.API.Tests.Controllers;

public class RestaurantsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public RestaurantsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task CreateRestaurant_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateRestaurantRequest
        {
            Name = "New Test Restaurant",
            Description = "A brand new restaurant",
            Address = new AddressRequest
            {
                Street = "123 Restaurant Lane",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50000",
                Country = "Malaysia"
            },
            MaxConcurrentOrders = 25
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/restaurants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>(_jsonOptions);
        restaurant.Should().NotBeNull();
        restaurant!.Name.Should().Be("New Test Restaurant");
        restaurant.IsActive.Should().BeTrue();
        restaurant.MaxConcurrentOrders.Should().Be(25);
    }

    [Fact]
    public async Task GetRestaurant_WithValidId_ShouldReturnRestaurant()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/restaurants/{restaurant.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<RestaurantDto>(_jsonOptions);
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(restaurant.Id.Value);
        dto.MenuItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetRestaurant_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/restaurants/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllRestaurants_ShouldReturnActiveRestaurants()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDataHelper.SeedRestaurantAsync(context);

        // Act
        var response = await _client.GetAsync("/api/restaurants");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var restaurants = await response.Content.ReadFromJsonAsync<List<RestaurantSummaryDto>>(_jsonOptions);
        restaurants.Should().NotBeNull();
        restaurants!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchRestaurants_WithMatchingName_ShouldReturnResults()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDataHelper.SeedRestaurantAsync(context);

        // Act
        var response = await _client.GetAsync("/api/restaurants/search?name=Test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var restaurants = await response.Content.ReadFromJsonAsync<List<RestaurantSummaryDto>>(_jsonOptions);
        restaurants.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateRestaurant_WithValidData_ShouldReturnUpdatedRestaurant()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        var request = new UpdateRestaurantRequest
        {
            Name = "Updated Restaurant Name",
            Description = "Updated description"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/restaurants/{restaurant.Id.Value}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<RestaurantDto>(_jsonOptions);
        dto!.Name.Should().Be("Updated Restaurant Name");
        dto.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task DeactivateRestaurant_ShouldSetIsActiveToFalse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        // Act
        var response = await _client.PostAsync($"/api/restaurants/{restaurant.Id.Value}/deactivate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<RestaurantDto>(_jsonOptions);
        dto!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateRestaurant_ShouldSetIsActiveToTrue()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);
        restaurant.Deactivate();
        context.Update(restaurant);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/restaurants/{restaurant.Id.Value}/activate", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<RestaurantDto>(_jsonOptions);
        dto!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task RateRestaurant_WithValidRating_ShouldUpdateRating()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        var request = new RateRestaurantRequest { Stars = 4.5m };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurant.Id.Value}/rate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<RestaurantDto>(_jsonOptions);
        dto!.AverageRating.Should().Be(4.5m);
        dto.TotalRatings.Should().Be(1);
    }

    [Fact]
    public async Task RateRestaurant_WithInvalidRating_ShouldReturnBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        var request = new RateRestaurantRequest { Stars = 6.0m }; // Invalid: > 5

        // Act
        var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurant.Id.Value}/rate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #region Menu Item Tests

    [Fact]
    public async Task GetMenuItems_ShouldReturnAllMenuItems()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/restaurants/{restaurant.Id.Value}/menu");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var menuItems = await response.Content.ReadFromJsonAsync<List<MenuItemDto>>(_jsonOptions);
        menuItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddMenuItem_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        var request = new AddMenuItemRequest
        {
            Name = "New Menu Item",
            Description = "A delicious new item",
            Price = 19.90m,
            Currency = "MYR",
            Category = "Specials",
            PreparationTimeMinutes = 20
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurant.Id.Value}/menu", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var menuItem = await response.Content.ReadFromJsonAsync<MenuItemDto>(_jsonOptions);
        menuItem.Should().NotBeNull();
        menuItem!.Name.Should().Be("New Menu Item");
        menuItem.Price.Should().Be(19.90m);
        menuItem.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task SetMenuItemAvailability_ShouldUpdateAvailability()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);
        var menuItem = restaurant.MenuItems.First();

        var request = new SetAvailabilityRequest { IsAvailable = false };

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/restaurants/{restaurant.Id.Value}/menu/{menuItem.Id.Value}/availability",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<MenuItemDto>(_jsonOptions);
        dto!.IsAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveMenuItem_ShouldReturnNoContent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);
        var menuItem = restaurant.MenuItems.First();

        // Act
        var response = await _client.DeleteAsync(
            $"/api/restaurants/{restaurant.Id.Value}/menu/{menuItem.Id.Value}"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify item was removed
        var getResponse = await _client.GetAsync($"/api/restaurants/{restaurant.Id.Value}/menu");
        var menuItems = await getResponse.Content.ReadFromJsonAsync<List<MenuItemDto>>(_jsonOptions);
        menuItems!.Should().HaveCount(2);
    }

    #endregion
}
