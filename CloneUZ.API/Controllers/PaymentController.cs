using CloneUZ.BLL.Interfaces;
using DTOs.Payment;
using Microsoft.AspNetCore.Mvc;

namespace CloneUZ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
    {
        var result = await _paymentService.ProcessPaymentAsync(request);
        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("refund")]
    public async Task<IActionResult> RefundPayment([FromBody] RefundRequestDto request)
    {
        var result = await _paymentService.RefundPaymentAsync(request);
        if (result.Status == "Failed")
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransactionStatus(string transactionId)
    {
        var result = await _paymentService.GetTransactionStatusAsync(transactionId);
        if (result == null)
        {
            return NotFound(new { status = "NotFound", message = "Транзакцію не знайдено" });
        }

        return Ok(result);
    }
    
}