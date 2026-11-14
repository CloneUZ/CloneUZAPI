namespace DTOs.Payment;

public class RefundRequestDto
{
    public string TransactionId { get; set; } = string.Empty;
    public double Amount { get; set; }
}