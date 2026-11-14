namespace DTOs.Payment;

public class PaymentRequestDto
{
    public Guid BookingId { get; set; }
    public double Amount { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Card";
}