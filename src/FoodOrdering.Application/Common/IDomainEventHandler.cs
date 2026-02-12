using FoodOrdering.Domain.Common;

namespace FoodOrdering.Application.Common;

/// <summary>
/// Interface for handling domain events.
/// </summary>
public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
