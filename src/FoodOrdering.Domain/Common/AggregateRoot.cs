namespace FoodOrdering.Domain.Common;

/// <summary>
/// Base class for all Aggregate Roots.
/// 
/// An Aggregate Root is:
/// - The entry point to an Aggregate
/// - The only object that outside objects can hold references to
/// - Responsible for ensuring consistency within the Aggregate
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Domain events raised by this aggregate.
    /// Events are dispatched after the aggregate is saved.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Add a domain event to be dispatched when the aggregate is saved.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Remove a domain event.
    /// </summary>
    protected void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Clear all domain events. Called after events are dispatched.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}