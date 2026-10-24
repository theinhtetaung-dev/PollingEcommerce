using Microsoft.AspNetCore.Mvc;
using PollingEcommerce.Domain;
using Polling.Shared;

namespace PollingEcommerce.WebAPI.Controllers;

[ApiController, Route("api/orders")]
public class OrdersController(IEcommerceService service) : ControllerBase
{
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request, CancellationToken token) => Map(await service.CheckoutAsync(request, token));

    [HttpGet("{orderNumber:guid}")]
    public async Task<IActionResult> Get(Guid orderNumber, CancellationToken token) => Map(await service.GetOrderAsync(orderNumber, token));

    private IActionResult Map<T>(Result<T> result) => result.IsSuccess ? Ok(result.Value) : result.Error.Code switch
    {
        "not_found" => NotFound(result.Error), "conflict" => Conflict(result.Error), _ => BadRequest(result.Error)
    };
}
