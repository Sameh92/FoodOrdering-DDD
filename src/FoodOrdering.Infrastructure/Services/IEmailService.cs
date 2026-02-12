namespace FoodOrdering.Infrastructure.Services;

/// <summary>
/// Interface for email service.
/// In a real application, this would be in the Application layer.
/// </summary>
public interface IEmailService
{
    Task SendOrderConfirmationAsync(Guid orderId, string customerEmail, string customerName);
    Task SendOrderStatusUpdateAsync(Guid orderId, string customerEmail, string status);
    Task SendOrderDeliveredAsync(Guid orderId, string customerEmail, string customerName);
    Task SendOrderCancelledAsync(Guid orderId, string customerEmail, string reason);
    Task SendWelcomeEmailAsync(string customerEmail, string customerName);
    Task SendPasswordResetAsync(string customerEmail, string resetToken);
    Task SendPromotionalEmailAsync(string customerEmail, string subject, string content);
}
