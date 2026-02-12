using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Ordering.Events;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Application.EventHandlers.OrderEventHandlers;

/// <summary>
/// Handles OrderDeliveredEvent - sends thank you and requests rating.
/// </summary>
public class SendThankYouWhenOrderDelivered : IDomainEventHandler<OrderDeliveredEvent>
{
    private readonly ILogger<SendThankYouWhenOrderDelivered> _logger;

    public SendThankYouWhenOrderDelivered(ILogger<SendThankYouWhenOrderDelivered> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderDeliveredEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Order {OrderId} delivered at {DeliveredAt}. Sending thank you to customer {CustomerId}",
            domainEvent.OrderId,
            domainEvent.DeliveredAt,
            domainEvent.CustomerId);

        // In real app: Send thank you email and request for rating
        // await _emailService.SendThankYouAndRatingRequest(...);

        return Task.CompletedTask;
    }
}
