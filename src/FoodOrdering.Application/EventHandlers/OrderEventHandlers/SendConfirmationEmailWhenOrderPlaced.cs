using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Ordering.Events;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Application.EventHandlers.OrderEventHandlers;

/// <summary>
/// Handles OrderPlacedEvent - sends confirmation email to customer.
/// </summary>
public class SendConfirmationEmailWhenOrderPlaced : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly ILogger<SendConfirmationEmailWhenOrderPlaced> _logger;

    public SendConfirmationEmailWhenOrderPlaced(ILogger<SendConfirmationEmailWhenOrderPlaced> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending confirmation email for order {OrderId} to customer {CustomerId}",
            domainEvent.OrderId,
            domainEvent.CustomerId);

        // In real app: Send email via email service
        // await _emailService.SendOrderConfirmation(domainEvent.CustomerId, domainEvent.OrderId, ...);

        return Task.CompletedTask;
    }
}
