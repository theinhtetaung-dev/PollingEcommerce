using Microsoft.AspNetCore.Mvc;
using PollingEcommerce.Domain;

namespace PollingEcommerce.WebAPI.Controllers;

[ApiController, Route("api/products")]
public class ProductsController(IEcommerceService service) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken token) => Ok(await service.GetProductsAsync(token));
}
