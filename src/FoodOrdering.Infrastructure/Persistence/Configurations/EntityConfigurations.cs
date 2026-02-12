using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.ValueObjects;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodOrdering.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Order aggregate.
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // Configure strongly typed ID
        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrderId.From(value))
            .HasColumnName("Id");

        builder.Property(o => o.CustomerId)
            .HasConversion(
                id => id.Value,
                value => CustomerId.From(value))
            .HasColumnName("CustomerId");

        builder.Property(o => o.RestaurantId)
            .HasConversion(
                id => id.Value,
                value => RestaurantId.From(value))
            .HasColumnName("RestaurantId");

        // Configure Address value object
        builder.OwnsOne(o => o.DeliveryAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("DeliveryStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("DeliveryCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("DeliveryState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("DeliveryPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("DeliveryCountry").HasMaxLength(100);
        });

        // Configure Money value object for delivery fee
        builder.OwnsOne(o => o.DeliveryFee, money =>
        {
            money.Property(m => m.Amount).HasColumnName("DeliveryFeeAmount").HasPrecision(18, 2);
            money.Property(m => m.Currency).HasColumnName("DeliveryFeeCurrency").HasMaxLength(3);
        });

        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(o => o.SpecialInstructions).HasMaxLength(500);
        builder.Property(o => o.CancellationReason).HasMaxLength(500);

        // Configure OrderItems collection
        builder.OwnsMany(o => o.Items, item =>
        {
            item.ToTable("OrderItems");
            item.WithOwner().HasForeignKey("OrderId");
            item.HasKey("Id");

            item.Property(i => i.MenuItemId)
                .HasConversion(
                    id => id.Value,
                    value => MenuItemId.From(value))
                .HasColumnName("MenuItemId");

            item.Property(i => i.Name).HasMaxLength(200);
            item.Property(i => i.SpecialInstructions).HasMaxLength(500);

            // Configure Price value object
            item.OwnsOne(i => i.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });
        });

        // Ignore domain events (not persisted)
        builder.Ignore(o => o.DomainEvents);

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.RestaurantId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PlacedAt);
    }
}

/// <summary>
/// EF Core configuration for Customer aggregate.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CustomerId.From(value))
            .HasColumnName("Id");

        // Configure CustomerName value object
        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(n => n.FirstName).HasColumnName("FirstName").HasMaxLength(100);
            name.Property(n => n.LastName).HasColumnName("LastName").HasMaxLength(100);
        });

        // Configure Email value object
        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value).HasColumnName("Email").HasMaxLength(200);
            email.HasIndex(e => e.Value).IsUnique();
        });

        // Configure PhoneNumber value object
        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(p => p.Value).HasColumnName("Phone").HasMaxLength(20);
        });

        // Configure optional default address
        builder.OwnsOne(c => c.DefaultDeliveryAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("DefaultStreet").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("DefaultCity").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("DefaultState").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("DefaultPostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("DefaultCountry").HasMaxLength(100);
        });

        builder.Property(c => c.MembershipLevel)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Ignore(c => c.DomainEvents);
    }
}

/// <summary>
/// EF Core configuration for Restaurant aggregate.
/// </summary>
public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => RestaurantId.From(value))
            .HasColumnName("Id");

        builder.Property(r => r.Name).HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(1000);

        // Configure Address value object
        builder.OwnsOne(r => r.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("State").HasMaxLength(100);
            address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(20);
            address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100);
        });

        // Configure Rating value object
        builder.OwnsOne(r => r.AverageRating, rating =>
        {
            rating.Property(rt => rt.Stars).HasColumnName("AverageRating").HasPrecision(3, 2);
        });

        // Configure OpeningHours as JSON
        builder.OwnsOne(r => r.OpeningHours, hours =>
        {
            hours.Property(h => h.OpenTime).HasColumnName("OpenTime");
            hours.Property(h => h.CloseTime).HasColumnName("CloseTime");
            hours.Property(h => h.OpenDays)
                .HasColumnName("OpenDays")
                .HasConversion(
                    days => string.Join(",", days.Select(d => (int)d)),
                    str => str.Split(",", StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => (DayOfWeek)int.Parse(s))
                              .ToArray());
        });

        // Configure MenuItems collection
        builder.OwnsMany(r => r.MenuItems, item =>
        {
            item.ToTable("MenuItems");
            item.WithOwner().HasForeignKey("RestaurantId");

            item.Property(i => i.Id)
                .HasConversion(
                    id => id.Value,
                    value => MenuItemId.From(value))
                .HasColumnName("Id");

            item.HasKey(i => i.Id);

            item.Property(i => i.Name).HasMaxLength(200);
            item.Property(i => i.Description).HasMaxLength(500);
            item.Property(i => i.Category).HasMaxLength(100);

            item.OwnsOne(i => i.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
                money.Property(m => m.Currency).HasColumnName("PriceCurrency").HasMaxLength(3);
            });
        });

        builder.Ignore(r => r.DomainEvents);

        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => r.IsActive);
    }
}