using FoodOrdering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
/// <summary>
/// Seed data for development/testing.
/// </summary>
public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Only seed if database is empty
        if (await context.Customers.AnyAsync())
            return;

        // Create sample customer
        var customer = FoodOrdering.Domain.Customers.Aggregates.Customer.Customer.Create(
            new FoodOrdering.Domain.Customers.ValueObjects.CustomerName("John", "Doe"),
            new FoodOrdering.Domain.Customers.ValueObjects.Email("john.doe@example.com"),
            new FoodOrdering.Domain.Customers.ValueObjects.PhoneNumber("+60123456789"),
            new FoodOrdering.Domain.Ordering.ValueObjects.Address(
                "123 Main Street",
                "Kuala Lumpur",
                "Wilayah Persekutuan",
                "50000",
                "Malaysia"
            )
        );

        await context.Customers.AddAsync(customer);

        // Create sample restaurant
        var restaurant = FoodOrdering.Domain.Restaurants.Aggregates.Restaurant.Restaurant.Create(
            "Burger Palace",
            "Best burgers in town!",
            new FoodOrdering.Domain.Ordering.ValueObjects.Address(
                "456 Food Street",
                "Kuala Lumpur",
                "Wilayah Persekutuan",
                "50100",
                "Malaysia"
            )
        );

        // Add menu items
        restaurant.AddMenuItem(
            "Classic Burger",
            "Juicy beef patty with lettuce, tomato, and special sauce",
            new FoodOrdering.Domain.Ordering.ValueObjects.Money(15.90m, "MYR"),
            "Burgers",
            15
        );

        restaurant.AddMenuItem(
            "Cheese Burger",
            "Classic burger with melted cheddar cheese",
            new FoodOrdering.Domain.Ordering.ValueObjects.Money(17.90m, "MYR"),
            "Burgers",
            15
        );

        restaurant.AddMenuItem(
            "French Fries",
            "Crispy golden fries",
            new FoodOrdering.Domain.Ordering.ValueObjects.Money(6.90m, "MYR"),
            "Sides",
            8
        );

        restaurant.AddMenuItem(
            "Soft Drink",
            "Choice of Coca-Cola, Sprite, or Fanta",
            new FoodOrdering.Domain.Ordering.ValueObjects.Money(4.50m, "MYR"),
            "Beverages",
            2
        );

        await context.Restaurants.AddAsync(restaurant);

        await context.SaveChangesAsync();

        Console.WriteLine($"Seeded Customer ID: {customer.Id}");
        Console.WriteLine($"Seeded Restaurant ID: {restaurant.Id}");
        Console.WriteLine("Menu Items:");
        foreach (var item in restaurant.MenuItems)
        {
            Console.WriteLine($"  - {item.Name} (ID: {item.Id}): {item.Price}");
        }
    }
}