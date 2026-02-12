namespace FoodOrdering.Domain.Common;

/// <summary>
/// Marker interface for all Domain Events.
/// Domain Events represent something important that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}

/// <summary>
/// Base record for Domain Events with common properties.
/// Using record for immutability and value-based equality.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}