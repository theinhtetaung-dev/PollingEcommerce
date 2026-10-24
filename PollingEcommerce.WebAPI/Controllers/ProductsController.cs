using Microsoft.AspNetCore.Mvc;
using PollingEcommerce.Domain;

namespace PollingEcommerce.WebAPI.Controllers;

[ApiController, Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IEcommerceService _service;

    public ProductsController(IEcommerceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var products = await _service.GetProductsAsync(token);
        return Ok(products);
    }
}
