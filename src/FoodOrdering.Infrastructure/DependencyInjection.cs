using FoodOrdering.Application.Common;
using FoodOrdering.Application.EventHandlers.OrderEventHandlers;
using FoodOrdering.Domain.Customers.Repositories;
using FoodOrdering.Domain.Ordering.Events;
using FoodOrdering.Domain.Ordering.Repositories;
using FoodOrdering.Domain.Restaurants.Repositories;
using FoodOrdering.Infrastructure.Persistence;
using FoodOrdering.Infrastructure.Persistence.Repositories;
using FoodOrdering.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FoodOrdering.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Register Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRestaurantRepository, RestaurantRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Register Domain Event Handlers
        services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, NotifyRestaurantWhenOrderPlaced>();
        services.AddScoped<IDomainEventHandler<OrderPlacedEvent>, SendConfirmationEmailWhenOrderPlaced>();
        services.AddScoped<IDomainEventHandler<OrderConfirmedEvent>, NotifyCustomerWhenOrderConfirmed>();
        services.AddScoped<IDomainEventHandler<OrderDeliveredEvent>, SendThankYouWhenOrderDelivered>();
        services.AddScoped<IDomainEventHandler<OrderCancelledEvent>, ProcessRefundWhenOrderCancelled>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}