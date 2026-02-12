using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Infrastructure.Persistence;

namespace FoodOrdering.API.Tests;

/// <summary>
/// Helper class for creating test data.
/// </summary>
public static class TestDataHelper
{
    public static async Task<Customer> SeedCustomerAsync(AppDbContext context)
    {
        var customer = Customer.Create(
            new CustomerName("Test", "Customer"),
            new Email("test@example.com"),
            new PhoneNumber("+60123456789"),
            new Address("123 Test Street", "Kuala Lumpur", "WP", "50000", "Malaysia")
        );

        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        return customer;
    }

    public static async Task<Restaurant> SeedRestaurantAsync(AppDbContext context)
    {
        var restaurant = Restaurant.Create(
            "Test Restaurant",
            "A restaurant for testing",
            new Address("456 Restaurant Street", "Kuala Lumpur", "WP", "50100", "Malaysia")
        );

        restaurant.AddMenuItem(
            "Test Burger",
            "Delicious test burger",
            new Money(15.90m, "MYR"),
            "Burgers",
            15
        );

        restaurant.AddMenuItem(
            "Test Fries",
            "Crispy test fries",
            new Money(6.90m, "MYR"),
            "Sides",
            10
        );

        restaurant.AddMenuItem(
            "Test Drink",
            "Refreshing test drink",
            new Money(4.50m, "MYR"),
            "Beverages",
            2
        );

        await context.Restaurants.AddAsync(restaurant);
        await context.SaveChangesAsync();

        return restaurant;
    }

    public static async Task<(Customer Customer, Restaurant Restaurant)> SeedAllAsync(AppDbContext context)
    {
        var customer = await SeedCustomerAsync(context);
        var restaurant = await SeedRestaurantAsync(context);

        return (customer, restaurant);
    }
}