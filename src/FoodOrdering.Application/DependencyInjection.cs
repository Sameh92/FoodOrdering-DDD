using FoodOrdering.Domain.Ordering.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FoodOrdering.Application;

/// <summary>
/// Extension methods for configuring Application layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        // Register Domain Services
        services.AddScoped<DeliveryFeeCalculator>();
        services.AddScoped<RestaurantAvailabilityService>();
        services.AddScoped<DeliveryTimeEstimator>();

        return services;
    }
}