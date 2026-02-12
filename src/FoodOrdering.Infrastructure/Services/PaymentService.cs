using FoodOrdering.Domain.Ordering.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FoodOrdering.Infrastructure.Services;

/// <summary>
/// Payment service implementation.
/// In production, this would integrate with services like:
/// - Stripe
/// - PayPal
/// - Square
/// - Local payment gateways (e.g., iPay88, MOLPay for Malaysia)
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ILogger<PaymentService> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation(
            "💳 Processing payment for Order {OrderId}. Amount: {Amount} {Currency}",
            request.OrderId,
            request.Amount.Amount,
            request.Amount.Currency
        );

        try
        {
            // Validate payment request
            ValidatePaymentRequest(request);

            // Simulate payment processing
            await Task.Delay(500); // Simulate network latency

            // In production:
            // - Stripe: await _stripeClient.PaymentIntents.CreateAsync(options);
            // - PayPal: await _paypalClient.CreateOrderAsync(order);

            // Simulate success (90% success rate for demo)
            var random = new Random();
            var isSuccess = random.Next(100) < 90;

            if (isSuccess)
            {
                var paymentId = $"PAY-{Guid.NewGuid().ToString()[..12].ToUpper()}";

                _logger.LogInformation(
                    "✅ Payment successful. PaymentId: {PaymentId}, OrderId: {OrderId}",
                    paymentId,
                    request.OrderId
                );

                return new PaymentResult
                {
                    IsSuccess = true,
                    PaymentId = paymentId,
                    TransactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    ProcessedAt = DateTime.UtcNow,
                    Message = "Payment processed successfully"
                };
            }
            else
            {
                _logger.LogWarning(
                    "❌ Payment failed for Order {OrderId}. Reason: Insufficient funds",
                    request.OrderId
                );

                return new PaymentResult
                {
                    IsSuccess = false,
                    ErrorCode = "INSUFFICIENT_FUNDS",
                    Message = "Payment declined. Please check your payment method."
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Payment processing error for Order {OrderId}",
                request.OrderId
            );

            return new PaymentResult
            {
                IsSuccess = false,
                ErrorCode = "PROCESSING_ERROR",
                Message = "An error occurred while processing your payment. Please try again."
            };
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        _logger.LogInformation(
            "💰 Processing refund for Payment {PaymentId}. Amount: {Amount} {Currency}",
            request.PaymentId,
            request.Amount.Amount,
            request.Amount.Currency
        );

        try
        {
            // Simulate refund processing
            await Task.Delay(300);

            // In production:
            // - Stripe: await _stripeClient.Refunds.CreateAsync(options);
            // - PayPal: await _paypalClient.RefundCapturedPaymentAsync(captureId, refund);

            var refundId = $"REF-{Guid.NewGuid().ToString()[..12].ToUpper()}";

            _logger.LogInformation(
                "✅ Refund successful. RefundId: {RefundId}, PaymentId: {PaymentId}",
                refundId,
                request.PaymentId
            );

            return new RefundResult
            {
                IsSuccess = true,
                RefundId = refundId,
                ProcessedAt = DateTime.UtcNow,
                Message = "Refund processed successfully. Funds will be returned within 3-5 business days."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "❌ Refund processing error for Payment {PaymentId}",
                request.PaymentId
            );

            return new RefundResult
            {
                IsSuccess = false,
                ErrorCode = "REFUND_ERROR",
                Message = "An error occurred while processing your refund. Please contact support."
            };
        }
    }

    public async Task<PaymentStatus> GetPaymentStatusAsync(string paymentId)
    {
        _logger.LogInformation(
            "🔍 Checking payment status for PaymentId: {PaymentId}",
            paymentId
        );

        // Simulate status check
        await Task.Delay(100);

        // In production, query payment provider for actual status

        return new PaymentStatus
        {
            PaymentId = paymentId,
            Status = "COMPLETED",
            LastUpdated = DateTime.UtcNow
        };
    }

    private static void ValidatePaymentRequest(PaymentRequest request)
    {
        if (request.OrderId == Guid.Empty)
            throw new ArgumentException("Order ID is required");

        if (request.Amount.Amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero");

        if (string.IsNullOrWhiteSpace(request.PaymentMethod))
            throw new ArgumentException("Payment method is required");
    }
}

#region Payment DTOs

public record PaymentRequest
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public Money Amount { get; init; } = null!;
    public string PaymentMethod { get; init; } = string.Empty; // "CARD", "EWALLET", "BANK_TRANSFER"
    public CardDetails? CardDetails { get; init; }
    public string? EWalletProvider { get; init; } // "GRABPAY", "TOUCHNGO", "BOOST"
}

public record CardDetails
{
    public string CardNumber { get; init; } = string.Empty; // Last 4 digits only for storage
    public string CardHolderName { get; init; } = string.Empty;
    public string ExpiryMonth { get; init; } = string.Empty;
    public string ExpiryYear { get; init; } = string.Empty;
    public string CardBrand { get; init; } = string.Empty; // "VISA", "MASTERCARD", etc.
}

public record PaymentResult
{
    public bool IsSuccess { get; init; }
    public string? PaymentId { get; init; }
    public string? TransactionId { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? ErrorCode { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record RefundRequest
{
    public string PaymentId { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public Money Amount { get; init; } = null!;
    public string Reason { get; init; } = string.Empty;
}

public record RefundResult
{
    public bool IsSuccess { get; init; }
    public string? RefundId { get; init; }
    public DateTime? ProcessedAt { get; init; }
    public string? ErrorCode { get; init; }
    public string Message { get; init; } = string.Empty;
}

public record PaymentStatus
{
    public string PaymentId { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty; // "PENDING", "COMPLETED", "FAILED", "REFUNDED"
    public DateTime LastUpdated { get; init; }
}

#endregion