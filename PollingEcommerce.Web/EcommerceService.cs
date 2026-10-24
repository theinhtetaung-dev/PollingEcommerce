using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Polling.Shared;
using PollingEcommerce.DataAccess.Models;
using PollingEcommerce.DataAccess.AppDbContext;

namespace PollingEcommerce.Domain;

public record ProductDto(long Id, string Name, string? Description, decimal Price, int Stock, int CategoryId, string Category);
public record CheckoutItem(long ProductId, int Quantity);
public record CheckoutRequest(long UserId, List<CheckoutItem> Items, string PaymentMode = "Success");
public record OrderResponse(Guid OrderNumber, decimal TotalAmount, string OrderStatus, string PaymentStatus, Guid? PaymentReference, string? FailureReason);
internal record PaymentCreateRequest(Guid MerchantOrderNumber, decimal Amount, string CurrencyCode, string TestMode, Guid IdempotencyKey);
internal record PaymentResponse(Guid PaymentReference, Guid MerchantOrderNumber, decimal Amount, string CurrencyCode, string Status, string? FailureReason);

public interface IEcommerceService
{
    Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken token);
    Task<Result<OrderResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken token);
    Task<Result<OrderResponse>> GetOrderAsync(Guid orderNumber, CancellationToken token);
}

public sealed class EcommerceService(PollingEcommerceDbContext dbContext, IHttpClientFactory httpClientFactory) : IEcommerceService
{
    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken token) =>
        await dbContext.tbl_products.AsNoTracking().Where(x => x.is_active && !x.delete_flag)
            .OrderBy(x => x.product_category.category_name).ThenBy(x => x.product_name)
            .Select(x => new ProductDto(x.product_id, x.product_name, x.description, x.unit_price, x.stock_quantity, x.product_category_id, x.product_category.category_name)).ToListAsync(token);

    public async Task<Result<OrderResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken token)
    {
        if (request.UserId <= 0 || request.Items.Count == 0 || request.Items.Any(x => x.Quantity <= 0))
            return Result<OrderResponse>.Failure("validation", "A customer and at least one item with a positive quantity are required.");
        if (!new[] { "Success", "Failed", "Delayed" }.Contains(request.PaymentMode))
            return Result<OrderResponse>.Failure("validation", "PaymentMode must be Success, Failed or Delayed.");
        if (!await dbContext.tbl_users.AnyAsync(x => x.user_id == request.UserId && !x.delete_flag, token))
            return Result<OrderResponse>.Failure("not_found", "Customer was not found.");

        var requested = request.Items.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.Sum(i => i.Quantity));
        var products = await dbContext.tbl_products.Where(x => requested.Keys.Contains(x.product_id)).ToListAsync(token);
        foreach (var item in requested)
        {
            var product = products.SingleOrDefault(x => x.product_id == item.Key);
            if (product is null) return Result<OrderResponse>.Failure("not_found", $"Product {item.Key} was not found.");
            if (!product.is_active || product.delete_flag) return Result<OrderResponse>.Failure("conflict", $"{product.product_name} is not available.");
            if (product.stock_quantity < item.Value) return Result<OrderResponse>.Failure("conflict", $"Insufficient stock for {product.product_name}.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(token);
        var order = new tbl_order { order_number = Guid.NewGuid(), user_id = request.UserId, total_amount = requested.Sum(x => products.Single(p => p.product_id == x.Key).unit_price * x.Value), order_status = "PendingPayment", add_date = DateTime.UtcNow };
        dbContext.tbl_orders.Add(order);
        await dbContext.SaveChangesAsync(token);
        foreach (var item in requested)
        {
            var product = products.Single(x => x.product_id == item.Key);
            product.stock_quantity -= item.Value; product.modify_date = DateTime.UtcNow;
            dbContext.tbl_order_items.Add(new tbl_order_item { order_id = order.order_id, product_id = product.product_id, quantity = item.Value, unit_price = product.unit_price, add_date = DateTime.UtcNow });
        }
        var paymentTransaction = new tbl_payment_transaction { order_id = order.order_id, attempt_number = 1, amount = order.total_amount, payment_status = "Pending", add_date = DateTime.UtcNow };
        dbContext.tbl_payment_transactions.Add(paymentTransaction);
        await dbContext.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        try
        {
            var client = httpClientFactory.CreateClient("PaymentApi");
            using var response = await client.PostAsJsonAsync("api/payments", new PaymentCreateRequest(order.order_number, order.total_amount, "MMK", request.PaymentMode, order.order_number), token);
            if (response.IsSuccessStatusCode)
            {
                var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken: token);
                paymentTransaction.payment_reference = payment?.PaymentReference; paymentTransaction.payment_status = payment?.Status ?? "Unknown"; paymentTransaction.modify_date = DateTime.UtcNow;
            }
            else paymentTransaction.payment_status = "Unknown";
        }
        catch (HttpRequestException) { paymentTransaction.payment_status = "Unknown"; }
        catch (TaskCanceledException) when (!token.IsCancellationRequested) { paymentTransaction.payment_status = "Unknown"; }
        await dbContext.SaveChangesAsync(token);
        return Result<OrderResponse>.Success(Map(order, paymentTransaction));
    }

    public async Task<Result<OrderResponse>> GetOrderAsync(Guid orderNumber, CancellationToken token)
    {
        var order = await dbContext.tbl_orders.Include(x => x.tbl_payment_transactions).SingleOrDefaultAsync(x => x.order_number == orderNumber && !x.delete_flag, token);
        if (order is null) return Result<OrderResponse>.Failure("not_found", "Order was not found.");
        var paymentTransaction = order.tbl_payment_transactions.OrderByDescending(x => x.attempt_number).First();
        if (order.order_status == "PendingPayment" && paymentTransaction.payment_reference.HasValue)
        {
            try
            {
                var payment = await httpClientFactory.CreateClient("PaymentApi").GetFromJsonAsync<PaymentResponse>($"api/payments/{paymentTransaction.payment_reference}", token);
                paymentTransaction.last_polled_date = DateTime.UtcNow;
                if (payment is not null)
                {
                    paymentTransaction.payment_status = payment.Status; paymentTransaction.failure_reason = payment.FailureReason; paymentTransaction.modify_date = DateTime.UtcNow;
                    if (payment.Status == "Succeeded") { order.order_status = "Paid"; paymentTransaction.completed_date = DateTime.UtcNow; }
                    else if (payment.Status == "Failed") { order.order_status = "PaymentFailed"; paymentTransaction.completed_date = DateTime.UtcNow; }
                    order.modify_date = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(token);
                }
            }
            catch (HttpRequestException) { paymentTransaction.payment_status = "Unknown"; paymentTransaction.last_polled_date = DateTime.UtcNow; await dbContext.SaveChangesAsync(token); }
            catch (TaskCanceledException) when (!token.IsCancellationRequested) { paymentTransaction.payment_status = "Unknown"; }
        }
        return Result<OrderResponse>.Success(Map(order, paymentTransaction));
    }

    private static OrderResponse Map(tbl_order order, tbl_payment_transaction payment) => new(order.order_number, order.total_amount, order.order_status, payment.payment_status, payment.payment_reference, payment.failure_reason);
}
