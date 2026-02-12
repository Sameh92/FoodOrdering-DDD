using FoodOrdering.Domain.Common;

namespace FoodOrdering.Application.Common;

/// <summary>
/// Interface for dispatching domain events.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
