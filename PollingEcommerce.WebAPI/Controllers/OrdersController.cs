using Microsoft.AspNetCore.Mvc;
using PollingEcommerce.Domain;
using Polling.Shared;

namespace PollingEcommerce.WebAPI.Controllers;

[ApiController, Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IEcommerceService _service;

    public OrdersController(IEcommerceService service)
    {
        _service = service;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request, CancellationToken token)
    {
        var result = await _service.CheckoutAsync(request, token);
        return Map(result);
    }

    [HttpGet("{orderNumber:guid}")]
    public async Task<IActionResult> Get(Guid orderNumber, CancellationToken token)
    {
        var result = await _service.GetOrderAsync(orderNumber, token);
        return Map(result);
    }

    private IActionResult Map<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error.Code switch
        {
            "not_found" => NotFound(result.Error),
            "conflict" => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }
}
