using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Ordering.Events;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Application.EventHandlers.OrderEventHandlers;

/// <summary>
/// Handles OrderConfirmedEvent - notifies customer.
/// </summary>
public class NotifyCustomerWhenOrderConfirmed : IDomainEventHandler<OrderConfirmedEvent>
{
    private readonly ILogger<NotifyCustomerWhenOrderConfirmed> _logger;

    public NotifyCustomerWhenOrderConfirmed(ILogger<NotifyCustomerWhenOrderConfirmed> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderConfirmedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Order {OrderId} confirmed by restaurant. Estimated ready time: {EstimatedReadyTime}",
            domainEvent.OrderId,
            domainEvent.EstimatedReadyTime);

        // In real app: Send push notification to customer
        // await _notificationService.NotifyCustomer(...);

        return Task.CompletedTask;
    }
}
