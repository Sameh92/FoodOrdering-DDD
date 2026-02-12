using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.Repositories;
using FoodOrdering.Domain.Customers.ValueObjects;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;
using FoodOrdering.Domain.Restaurants.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FoodOrdering.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Order aggregate.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPendingOrdersForRestaurantAsync(RestaurantId restaurantId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.RestaurantId == restaurantId)
            .Where(o => o.Status == OrderStatus.PendingConfirmation)
            .OrderBy(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetActiveOrdersForRestaurantAsync(RestaurantId restaurantId, CancellationToken cancellationToken = default)
    {
        var activeStatuses = new[]
        {
            OrderStatus.PendingConfirmation,
            OrderStatus.Confirmed,
            OrderStatus.Preparing,
            OrderStatus.ReadyForPickup
        };

        return await _context.Orders
            .Where(o => o.RestaurantId == restaurantId)
            .Where(o => activeStatuses.Contains(o.Status))
            .OrderBy(o => o.PlacedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
    }

    public void Update(Order order)
    {
        _context.Orders.Update(order);
    }

    public void Remove(Order order)
    {
        _context.Orders.Remove(order);
    }
}

/// <summary>
/// Repository implementation for Customer aggregate.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email.Value == email.Value, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(c => c.Email.Value == email.Value, cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public void Remove(Customer customer)
    {
        _context.Customers.Remove(customer);
    }
}

/// <summary>
/// Repository implementation for Restaurant aggregate.
/// </summary>
public class RestaurantRepository : IRestaurantRepository
{
    private readonly AppDbContext _context;

    public RestaurantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Restaurant?> GetByIdAsync(RestaurantId id, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Restaurant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Restaurant>> GetActiveRestaurantsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Restaurant>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Where(r => r.IsActive)
            .Where(r => r.Name.Contains(searchTerm))
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Restaurant>> GetByCityAsync(string city, CancellationToken cancellationToken = default)
    {
        return await _context.Restaurants
            .Where(r => r.IsActive)
            .Where(r => r.Address.City == city)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        await _context.Restaurants.AddAsync(restaurant, cancellationToken);
    }

    public void Update(Restaurant restaurant)
    {
        _context.Restaurants.Update(restaurant);
    }

    public void Remove(Restaurant restaurant)
    {
        _context.Restaurants.Remove(restaurant);
    }
}