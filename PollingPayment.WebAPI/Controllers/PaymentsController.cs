using Microsoft.AspNetCore.Mvc;
using PollingPayment.Domain.Features.Payment;
using PollingPayment.Domain.Models.Payment;

namespace PollingPayment.WebAPI.Controllers;

[ApiController, Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken token)
    {
        var result = await _paymentService.CreateAsync(request, token);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Error.Code == "conflict")
        {
            return Conflict(result.Error);
        }

        return BadRequest(result.Error);
    }

    [HttpGet("{reference:guid}")]
    public async Task<IActionResult> Get(Guid reference, CancellationToken token)
    {
        var result = await _paymentService.GetAsync(reference, token);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return NotFound(result.Error);
    }
}
