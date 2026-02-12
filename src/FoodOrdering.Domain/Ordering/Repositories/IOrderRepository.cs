using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Ordering.Aggregates.Order;
using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.Domain.Ordering.Repositories;

/// <summary>
/// Repository interface for Order Aggregate.
/// Interface lives in Domain, implementation in Infrastructure.
/// </summary>
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(OrderId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(CustomerId customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetPendingOrdersForRestaurantAsync(RestaurantId restaurantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetActiveOrdersForRestaurantAsync(RestaurantId restaurantId, CancellationToken cancellationToken = default);
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
    void Update(Order order);
    void Remove(Order order);
}