using FoodOrdering.Domain.Customers.Aggregates.Customer;
using FoodOrdering.Domain.Customers.ValueObjects;

namespace FoodOrdering.Domain.Customers.Repositories;

/// <summary>
/// Repository interface for Customer Aggregate.
/// </summary>
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    void Update(Customer customer);
    void Remove(Customer customer);
}