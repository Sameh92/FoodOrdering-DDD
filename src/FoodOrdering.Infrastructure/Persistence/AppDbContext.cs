using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.Infrastructure.Persistence;

/// <summary>
/// Application database context.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}