using FoodOrdering.Application.Common;
using FoodOrdering.Domain.Ordering.Events;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Application.EventHandlers.OrderEventHandlers;

/// <summary>
/// Handles OrderPlacedEvent - sends notification to restaurant.
/// </summary>
public class NotifyRestaurantWhenOrderPlaced : IDomainEventHandler<OrderPlacedEvent>
{
    private readonly ILogger<NotifyRestaurantWhenOrderPlaced> _logger;

    public NotifyRestaurantWhenOrderPlaced(ILogger<NotifyRestaurantWhenOrderPlaced> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderPlacedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notifying restaurant {RestaurantId} about new order {OrderId}",
            domainEvent.RestaurantId,
            domainEvent.OrderId);

        // In real app: Send push notification, SMS, or email to restaurant
        // await _notificationService.NotifyRestaurant(domainEvent.RestaurantId, ...);

        return Task.CompletedTask;
    }
}
