using System.Collections.Concurrent;
using CloneUZ.BLL.Interfaces;
using DTOs.Payment;

namespace CloneUZ.BLL.Services;

public class PaymentService : IPaymentService
{
    private static readonly ConcurrentDictionary<string, PaymentResponseDto> _transactions = new();

    public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto request)
    {
        await Task.Delay(2000);

        if (request.Amount <= 0 || string.IsNullOrWhiteSpace(request.CardNumber))
        {
            return new PaymentResponseDto
            {
                Status = "Failed",
                Timestamp = DateTime.UtcNow,
                Message = "Некоректні платіжні дані"
            };
        }

        var transactionId = $"TXN-UZ-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var response = new PaymentResponseDto
        {
            Status = "Success",
            TransactionId = transactionId,
            Timestamp = DateTime.UtcNow,
            Message = "Оплата пройшла успішно."
        };

        _transactions[transactionId] = response;
        return response;
    }

    public async Task<RefundResponseDto> RefundPaymentAsync(RefundRequestDto request)
    {
        await Task.Delay(1500);

        if (string.IsNullOrWhiteSpace(request.TransactionId))
        {
            return new RefundResponseDto
            {
                Status = "Failed",
                Timestamp = DateTime.UtcNow,
                Message = "Вкажіть TransactionId"
            };
        }

        if (_transactions.TryGetValue(request.TransactionId.Trim(), out var existing))
        {
            existing.Status = "Refunded";
            existing.Message = "Кошти за цією транзакцією повернуто.";
        }

        var refundId = $"REF-UZ-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        return new RefundResponseDto
        {
            Status = "Success",
            RefundTransactionId = refundId,
            OriginalTransactionId = request.TransactionId,
            Timestamp = DateTime.UtcNow,
            Message = "Кошти успішно повернуто."
        };
    }

    public Task<PaymentResponseDto?> GetTransactionStatusAsync(string transactionId)
    {
        _transactions.TryGetValue(transactionId.Trim(), out var transaction);
        return Task.FromResult(transaction);
    }
}