using Polling.Shared;
using PollingEcommerce.Domain.Models.Orders;
using PollingEcommerce.Domain.Models.Products;

namespace PollingEcommerce.Domain.Features.Store;

public interface IEcommerceService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken token);

    Task<Result<OrderResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken token);

    Task<Result<OrderResponse>> GetOrderAsync(Guid orderNumber, CancellationToken token);
}
