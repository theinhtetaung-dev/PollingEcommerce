using Microsoft.AspNetCore.Mvc;
using PollingEcommerce.MVC.Models;
using PollingEcommerce.MVC.Models.Orders;
using PollingEcommerce.MVC.Models.Store;
using System.Net.Http.Json;
using System.Diagnostics;

namespace PollingEcommerce.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(CancellationToken token)
        {
            var products = await _httpClientFactory.CreateClient("EcommerceApi").GetFromJsonAsync<List<ProductViewModel>>("api/products", token) ?? [];
            return View(products);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutForm form, CancellationToken token)
        {
            var items = form.Items.Where(x => x.Quantity > 0).ToList();
            if (items.Count == 0)
            {
                TempData["Error"] = "Please select at least one product.";
                return RedirectToAction(nameof(Index));
            }

            var response = await _httpClientFactory.CreateClient("EcommerceApi").PostAsJsonAsync("api/orders/checkout", new { userId = 1, items, paymentMode = form.PaymentMode }, token);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorViewModel>(cancellationToken: token);
                TempData["Error"] = error?.Message ?? "Checkout could not be completed. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            var order = await response.Content.ReadFromJsonAsync<OrderViewModel>(cancellationToken: token);
            if (order is null)
            {
                TempData["Error"] = "The order response was incomplete. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Status), new { orderNumber = order.OrderNumber });
        }

        public IActionResult Status(Guid orderNumber)
        {
            return View(new OrderViewModel
            {
                OrderNumber = orderNumber
            });
        }

        [HttpGet]
        public async Task<IActionResult> OrderStatus(Guid orderNumber, CancellationToken token)
        {
            var response = await _httpClientFactory.CreateClient("EcommerceApi").GetAsync($"api/orders/{orderNumber}", token);
            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                ContentType = "application/json",
                Content = await response.Content.ReadAsStringAsync(token)
            };
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
