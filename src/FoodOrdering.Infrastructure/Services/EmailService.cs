using Microsoft.Extensions.Logging;

namespace FoodOrdering.Infrastructure.Services;

/// <summary>
/// Email service implementation.
/// In production, this would integrate with services like:
/// - SendGrid
/// - AWS SES
/// - Mailgun
/// - SMTP server
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(Guid orderId, string customerEmail, string customerName)
    {
        // In production: integrate with email provider
        _logger.LogInformation(
            "📧 Sending order confirmation email to {Email} for order {OrderId}",
            customerEmail,
            orderId
        );

        var emailContent = BuildOrderConfirmationEmail(orderId, customerName);

        await SendEmailAsync(
            to: customerEmail,
            subject: $"Order Confirmed - #{orderId.ToString()[..8]}",
            body: emailContent
        );

        _logger.LogInformation(
            "✅ Order confirmation email sent to {Email}",
            customerEmail
        );
    }

    public async Task SendOrderStatusUpdateAsync(Guid orderId, string customerEmail, string status)
    {
        _logger.LogInformation(
            "📧 Sending order status update to {Email}. Order: {OrderId}, Status: {Status}",
            customerEmail,
            orderId,
            status
        );

        var emailContent = BuildStatusUpdateEmail(orderId, status);

        await SendEmailAsync(
            to: customerEmail,
            subject: $"Order Update - #{orderId.ToString()[..8]} is now {status}",
            body: emailContent
        );

        _logger.LogInformation(
            "✅ Order status update email sent to {Email}",
            customerEmail
        );
    }

    public async Task SendOrderDeliveredAsync(Guid orderId, string customerEmail, string customerName)
    {
        _logger.LogInformation(
            "📧 Sending order delivered email to {Email} for order {OrderId}",
            customerEmail,
            orderId
        );

        var emailContent = BuildOrderDeliveredEmail(orderId, customerName);

        await SendEmailAsync(
            to: customerEmail,
            subject: $"Order Delivered! - #{orderId.ToString()[..8]}",
            body: emailContent
        );

        _logger.LogInformation(
            "✅ Order delivered email sent to {Email}",
            customerEmail
        );
    }

    public async Task SendOrderCancelledAsync(Guid orderId, string customerEmail, string reason)
    {
        _logger.LogInformation(
            "📧 Sending order cancellation email to {Email} for order {OrderId}",
            customerEmail,
            orderId
        );

        var emailContent = BuildOrderCancelledEmail(orderId, reason);

        await SendEmailAsync(
            to: customerEmail,
            subject: $"Order Cancelled - #{orderId.ToString()[..8]}",
            body: emailContent
        );

        _logger.LogInformation(
            "✅ Order cancellation email sent to {Email}",
            customerEmail
        );
    }

    public async Task SendWelcomeEmailAsync(string customerEmail, string customerName)
    {
        _logger.LogInformation(
            "📧 Sending welcome email to {Email}",
            customerEmail
        );

        var emailContent = BuildWelcomeEmail(customerName);

        await SendEmailAsync(
            to: customerEmail,
            subject: "Welcome to Food Ordering!",
            body: emailContent
        );

        _logger.LogInformation(
            "✅ Welcome email sent to {Email}",
            customerEmail
        );
    }

    public async Task SendPasswordResetAsync(string customerEmail, string resetToken)
    {
        _logger.LogInformation(
            "📧 Sending password reset email to {Email}",
            customerEmail
        );

        var emailContent = BuildPasswordResetEmail(resetToken);

        await SendEmailAsync(
            to: customerEmail,
            subject: "Password Reset Request",
            body: emailContent
        );

        _logger.LogInformation(
            "✅ Password reset email sent to {Email}",
            customerEmail
        );
    }

    public async Task SendPromotionalEmailAsync(string customerEmail, string subject, string content)
    {
        _logger.LogInformation(
            "📧 Sending promotional email to {Email}",
            customerEmail
        );

        await SendEmailAsync(
            to: customerEmail,
            subject: subject,
            body: content
        );

        _logger.LogInformation(
            "✅ Promotional email sent to {Email}",
            customerEmail
        );
    }

    #region Private Methods

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        // Simulate network delay
        await Task.Delay(100);

        // In production, this would be:
        // - SendGrid: await _sendGridClient.SendEmailAsync(msg);
        // - AWS SES: await _sesClient.SendEmailAsync(request);
        // - SMTP: await _smtpClient.SendMailAsync(message);

        _logger.LogDebug(
            "Email sent - To: {To}, Subject: {Subject}",
            to,
            subject
        );
    }

    private static string BuildOrderConfirmationEmail(Guid orderId, string customerName)
    {
        return $@"
            <html>
            <body>
                <h1>Order Confirmed!</h1>
                <p>Dear {customerName},</p>
                <p>Thank you for your order!</p>
                <p>Your order <strong>#{orderId.ToString()[..8]}</strong> has been confirmed and is being prepared.</p>
                <p>We'll notify you when it's ready for pickup.</p>
                <br/>
                <p>Best regards,</p>
                <p>Food Ordering Team</p>
            </body>
            </html>
        ";
    }

    private static string BuildStatusUpdateEmail(Guid orderId, string status)
    {
        return $@"
            <html>
            <body>
                <h1>Order Status Update</h1>
                <p>Your order <strong>#{orderId.ToString()[..8]}</strong> status has been updated.</p>
                <p>Current Status: <strong>{status}</strong></p>
                <br/>
                <p>Best regards,</p>
                <p>Food Ordering Team</p>
            </body>
            </html>
        ";
    }

    private static string BuildOrderDeliveredEmail(Guid orderId, string customerName)
    {
        return $@"
            <html>
            <body>
                <h1>Order Delivered! 🎉</h1>
                <p>Dear {customerName},</p>
                <p>Your order <strong>#{orderId.ToString()[..8]}</strong> has been delivered!</p>
                <p>We hope you enjoy your meal!</p>
                <p>Please take a moment to rate your experience.</p>
                <br/>
                <p>Thank you for ordering with us!</p>
                <p>Food Ordering Team</p>
            </body>
            </html>
        ";
    }

    private static string BuildOrderCancelledEmail(Guid orderId, string reason)
    {
        return $@"
            <html>
            <body>
                <h1>Order Cancelled</h1>
                <p>Your order <strong>#{orderId.ToString()[..8]}</strong> has been cancelled.</p>
                <p>Reason: {reason}</p>
                <p>If you were charged, a refund will be processed within 3-5 business days.</p>
                <br/>
                <p>We apologize for any inconvenience.</p>
                <p>Food Ordering Team</p>
            </body>
            </html>
        ";
    }

    private static string BuildWelcomeEmail(string customerName)
    {
        return $@"
            <html>
            <body>
                <h1>Welcome to Food Ordering! 🍕</h1>
                <p>Dear {customerName},</p>
                <p>Thank you for joining Food Ordering!</p>
                <p>We're excited to have you as part of our community.</p>
                <h2>What's Next?</h2>
                <ul>
                    <li>Browse restaurants near you</li>
                    <li>Place your first order</li>
                    <li>Earn rewards with every order</li>
                </ul>
                <br/>
                <p>Happy ordering!</p>
                <p>Food Ordering Team</p>
            </body>
            </html>
        ";
    }

    private static string BuildPasswordResetEmail(string resetToken)
    {
        return $@"
            <html>
            <body>
                <h1>Password Reset Request</h1>
                <p>We received a request to reset your password.</p>
                <p>Click the link below to reset your password:</p>
                <p><a href=""https://foodordering.com/reset-password?token={resetToken}"">Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this, please ignore this email.</p>
                <br/>
                <p>Food Ordering Team</p>
            </body>
            </html>
        ";
    }

    #endregion
}