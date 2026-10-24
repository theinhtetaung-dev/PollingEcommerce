using Microsoft.AspNetCore.Mvc;
using PollingPayment.Domain;

namespace PollingPayment.WebAPI.Controllers;

[ApiController, Route("api/payments")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePaymentRequest request, CancellationToken token)
    {
        var result = await paymentService.CreateAsync(request, token);
        if (result.IsSuccess) return Ok(result.Value);
        return result.Error.Code == "conflict" ? Conflict(result.Error) : BadRequest(result.Error);
    }

    [HttpGet("{reference:guid}")]
    public async Task<IActionResult> Get(Guid reference, CancellationToken token)
    {
        var result = await paymentService.GetAsync(reference, token);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }
}
