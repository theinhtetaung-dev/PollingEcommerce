using Polling.Shared;

namespace PollingEcommerce.Domain;

public interface IEcommerceService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken token);

    Task<Result<OrderResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken token);

    Task<Result<OrderResponse>> GetOrderAsync(Guid orderNumber, CancellationToken token);
}
