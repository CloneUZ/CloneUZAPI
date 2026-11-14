using DTOs.Payment;

namespace CloneUZ.BLL.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto request);
    Task<RefundResponseDto> RefundPaymentAsync(RefundRequestDto request);
    Task<PaymentResponseDto?> GetTransactionStatusAsync(string transactionId);
}