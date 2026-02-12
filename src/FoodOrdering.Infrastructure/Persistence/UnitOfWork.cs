using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Common;

namespace FoodOrdering.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation that handles transactions and domain event dispatching.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UnitOfWork(AppDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all domain events from aggregates before saving
        var domainEvents = new List<IDomainEvent>();

        foreach (var entry in _context.ChangeTracker.Entries())
        {
            if (entry.Entity is AggregateRoot<dynamic> aggregate && aggregate.DomainEvents.Any())
            {
                domainEvents.AddRange(aggregate.DomainEvents);
                aggregate.ClearDomainEvents();
            }
        }

        // Save changes to database
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch domain events after successful save
        await _eventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        return result;
    }
}