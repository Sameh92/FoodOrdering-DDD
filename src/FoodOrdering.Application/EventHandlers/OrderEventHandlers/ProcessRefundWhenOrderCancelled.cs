using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Ordering.Events;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Application.EventHandlers.OrderEventHandlers;

/// <summary>
/// Handles OrderCancelledEvent - processes refund and notifications.
/// </summary>
public class ProcessRefundWhenOrderCancelled : IDomainEventHandler<OrderCancelledEvent>
{
    private readonly ILogger<ProcessRefundWhenOrderCancelled> _logger;

    public ProcessRefundWhenOrderCancelled(ILogger<ProcessRefundWhenOrderCancelled> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Order {OrderId} cancelled. Reason: {Reason}. Processing refund for customer {CustomerId}",
            domainEvent.OrderId,
            domainEvent.Reason,
            domainEvent.CustomerId);

        // In real app: Process refund via payment service
        // await _paymentService.ProcessRefund(...);

        return Task.CompletedTask;
    }
}