using FoodOrdering.Domain.Restaurants.Aggregates.Restaurant;

namespace FoodOrdering.Domain.Restaurants.Repositories;

/// <summary>
/// Repository interface for Restaurant Aggregate.
/// </summary>
public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(RestaurantId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Restaurant>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Restaurant>> GetActiveRestaurantsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Restaurant>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Restaurant>> GetByCityAsync(string city, CancellationToken cancellationToken = default);
    Task AddAsync(Restaurant restaurant, CancellationToken cancellationToken = default);
    void Update(Restaurant restaurant);
    void Remove(Restaurant restaurant);
}