namespace FoodOrdering.Infrastructure.Services;

/// <summary>
/// Interface for payment service.
/// In a real application, this would be in the Application layer.
/// </summary>
public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<RefundResult> ProcessRefundAsync(RefundRequest request);
    Task<PaymentStatus> GetPaymentStatusAsync(string paymentId);
}
