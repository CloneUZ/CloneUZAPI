namespace DTOs.Payment;

public class PaymentResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}