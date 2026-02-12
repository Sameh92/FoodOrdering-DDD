using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Infrastructure.Services;

/// <summary>
/// Domain Event Dispatcher that finds and invokes handlers for domain events.
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        _logger.LogDebug("Dispatching domain event {EventType}", eventType.Name);

        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var method = handlerType.GetMethod("HandleAsync");
            if (method != null)
            {
                try
                {
                    var task = (Task?)method.Invoke(handler, new object[] { domainEvent, cancellationToken });
                    if (task != null)
                    {
                        await task;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling domain event {EventType}", eventType.Name);
                    // In production, you might want to handle this differently
                    // e.g., retry, dead letter queue, etc.
                }
            }
        }
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await DispatchAsync(domainEvent, cancellationToken);
        }
    }
}