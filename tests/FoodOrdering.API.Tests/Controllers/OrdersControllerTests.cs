using FluentAssertions;
using FoodOrdering.API.Controllers;
using FoodOrdering.API.DTOs.Common;
using FoodOrdering.API.DTOs.OrdersDTOs;
using FoodOrdering.Application.Orders.DTOs;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using FoodOrdering.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace FoodOrdering.API.Tests.Controllers;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public OrdersControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    [Fact]
    public async Task PlaceOrder_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (customer, restaurant) = await TestDataHelper.SeedAllAsync(context);

        var menuItem = restaurant.MenuItems.First();
        await MockUpdateResturantOpeningHours(restaurant, context);


        var request = new PlaceOrderRequest
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressRequest
            {
                Street = "789 Delivery Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50200",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequestDto>
            {
                new()
                {
                    MenuItemId = menuItem.Id.Value,
                    Quantity = 2
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var order = JsonSerializer.Deserialize<OrderDto>(content, _jsonOptions);

        order.Should().NotBeNull();
        order!.Status.Should().Be("PendingConfirmation");
        order.Items.Should().HaveCount(1);
        order.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task PlaceOrder_WithInvalidCustomer_ShouldReturnBadRequest()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var restaurant = await TestDataHelper.SeedRestaurantAsync(context);

        var request = new PlaceOrderRequest
        {
            CustomerId = Guid.NewGuid(), // Non-existent customer
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressRequest
            {
                Street = "789 Delivery Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50200",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequestDto>
            {
                new()
                {
                    MenuItemId = restaurant.MenuItems.First().Id.Value,
                    Quantity = 1
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetOrder_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (customer, restaurant) = await TestDataHelper.SeedAllAsync(context);
        
        await MockUpdateResturantOpeningHours(restaurant, context);

        // First, create an order
        var menuItem = restaurant.MenuItems.First();
        var placeRequest = new PlaceOrderRequest
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressRequest
            {
                Street = "789 Delivery Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50200",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequestDto>
            {
                new() { MenuItemId = menuItem.Id.Value, Quantity = 1 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", placeRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        order.Should().NotBeNull();
        order!.Id.Should().Be(createdOrder.Id);
    }

    [Fact]
    public async Task GetOrder_WithInvalidId_ShouldReturnNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ConfirmOrder_WithValidPendingOrder_ShouldReturnOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (customer, restaurant) = await TestDataHelper.SeedAllAsync(context);

        await MockUpdateResturantOpeningHours(restaurant, context);
        // Create an order first
        var menuItem = restaurant.MenuItems.First();
        var placeRequest = new PlaceOrderRequest
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressRequest
            {
                Street = "789 Delivery Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50200",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequestDto>
            {
                new() { MenuItemId = menuItem.Id.Value, Quantity = 1 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", placeRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);

        var confirmRequest = new ConfirmOrderRequest
        {
            EstimatedPrepTimeMinutes = 25
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{createdOrder!.Id}/confirm", confirmRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        order!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CancelOrder_WithValidPendingOrder_ShouldReturnOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (customer, restaurant) = await TestDataHelper.SeedAllAsync(context);

        await MockUpdateResturantOpeningHours(restaurant, context);
        // Create an order first
        var menuItem = restaurant.MenuItems.First();
        var placeRequest = new PlaceOrderRequest
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressRequest
            {
                Street = "789 Delivery Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50200",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequestDto>
            {
                new() { MenuItemId = menuItem.Id.Value, Quantity = 1 }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/orders", placeRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);

        var cancelRequest = new CancelOrderRequest
        {
            Reason = "Changed my mind"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/orders/{createdOrder!.Id}/cancel", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var order = await response.Content.ReadFromJsonAsync<OrderDto>(_jsonOptions);
        order!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetCustomerOrders_ShouldReturnCustomerOrders()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (customer, restaurant) = await TestDataHelper.SeedAllAsync(context);

        await MockUpdateResturantOpeningHours(restaurant, context);
        // Create multiple orders
        var menuItem = restaurant.MenuItems.First();
        var placeRequest = new PlaceOrderRequest
        {
            CustomerId = customer.Id.Value,
            RestaurantId = restaurant.Id.Value,
            DeliveryAddress = new AddressRequest
            {
                Street = "789 Delivery Street",
                City = "Kuala Lumpur",
                State = "WP",
                PostalCode = "50200",
                Country = "Malaysia"
            },
            Items = new List<OrderItemRequestDto>
            {
                new() { MenuItemId = menuItem.Id.Value, Quantity = 1 }
            }
        };

        await _client.PostAsJsonAsync("/api/orders", placeRequest);
        await _client.PostAsJsonAsync("/api/orders", placeRequest);

        // Act
        var response = await _client.GetAsync($"/api/orders/customer/{customer.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var orders = await response.Content.ReadFromJsonAsync<List<OrderSummaryDto>>(_jsonOptions);
        orders.Should().NotBeNull();
        orders!.Count.Should().BeGreaterOrEqualTo(2);
    }

    private async Task MockUpdateResturantOpeningHours(Restaurant restaurant, AppDbContext context)
    {
        restaurant.UpdateOpeningHours(new OpeningHours(
            new TimeOnly(0, 0),
            new TimeOnly(23, 59),
            Enum.GetValues<DayOfWeek>().ToArray()));

        context.Update(restaurant);
        await context.SaveChangesAsync();
    }
}