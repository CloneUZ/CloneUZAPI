namespace DTOs.Payment;

public class RefundResponseDto
{
    public string Status { get; set; } = string.Empty;
    public string? RefundTransactionId { get; set; }
    public string? OriginalTransactionId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}