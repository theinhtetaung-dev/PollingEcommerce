using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Polling.Shared;
using PollingEcommerce.DataAccess.Models;
using PollingEcommerce.DataAccess.AppDbContext;
using PollingEcommerce.Domain.Models.Orders;
using PollingEcommerce.Domain.Models.Payments;
using PollingEcommerce.Domain.Models.Products;

namespace PollingEcommerce.Domain.Features.Store;

public sealed class EcommerceService : IEcommerceService
{
    private readonly PollingEcommerceDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public EcommerceService(PollingEcommerceDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(CancellationToken token)
    {
        return await _dbContext.tbl_products
            .AsNoTracking()
            .Where(x => x.is_active && !x.delete_flag)
            .OrderBy(x => x.product_category.category_name).ThenBy(x => x.product_name)
            .Select(x => new ProductDto(x.product_id, x.product_name, x.description, x.unit_price, x.stock_quantity, x.product_category_id, x.product_category.category_name)).ToListAsync(token);
    }

    public async Task<Result<OrderResponse>> CheckoutAsync(CheckoutRequest request, CancellationToken token)
    {
        if (request.UserId <= 0 || request.Items.Count == 0 || request.Items.Any(x => x.Quantity <= 0))
        {
            return Result<OrderResponse>.Failure("validation", "A customer and at least one item with a positive quantity are required.");
        }

        if (!new[] { "Success", "Failed", "Delayed" }.Contains(request.PaymentMode))
        {
            return Result<OrderResponse>.Failure("validation", "PaymentMode must be Success, Failed or Delayed.");
        }

        if (!await _dbContext.tbl_users.AnyAsync(x => x.user_id == request.UserId && !x.delete_flag, token))
        {
            return Result<OrderResponse>.Failure("not_found", "Customer was not found.");
        }

        var requested = request.Items.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.Sum(i => i.Quantity));
        var products = await _dbContext.tbl_products.Where(x => requested.Keys.Contains(x.product_id)).ToListAsync(token);
        foreach (var item in requested)
        {
            var product = products.SingleOrDefault(x => x.product_id == item.Key);
            if (product is null)
            {
                return Result<OrderResponse>.Failure("not_found", $"Product {item.Key} was not found.");
            }

            if (!product.is_active || product.delete_flag)
            {
                return Result<OrderResponse>.Failure("conflict", $"{product.product_name} is not available.");
            }

            if (product.stock_quantity < item.Value)
            {
                return Result<OrderResponse>.Failure("conflict", $"Insufficient stock for {product.product_name}.");
            }
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(token);
        var order = new tbl_order
        {
            order_number = Guid.NewGuid(),
            user_id = request.UserId,
            total_amount = requested.Sum(x => products.Single(p => p.product_id == x.Key).unit_price * x.Value),
            order_status = "PendingPayment",
            add_date = DateTime.UtcNow
        };
        _dbContext.tbl_orders.Add(order);
        await _dbContext.SaveChangesAsync(token);
        foreach (var item in requested)
        {
            var product = products.Single(x => x.product_id == item.Key);
            product.stock_quantity -= item.Value;
            product.modify_date = DateTime.UtcNow;
            _dbContext.tbl_order_items.Add(new tbl_order_item
            {
                order_id = order.order_id,
                product_id = product.product_id,
                quantity = item.Value,
                unit_price = product.unit_price,
                add_date = DateTime.UtcNow
            });
        }

        var paymentTransaction = new tbl_payment_transaction
        {
            order_id = order.order_id,
            attempt_number = 1,
            amount = order.total_amount,
            payment_status = "Pending",
            add_date = DateTime.UtcNow
        };
        _dbContext.tbl_payment_transactions.Add(paymentTransaction);
        await _dbContext.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        try
        {
            var client = _httpClientFactory.CreateClient("PaymentApi");
            using var response = await client.PostAsJsonAsync("api/payments", new PaymentCreateRequest(order.order_number, order.total_amount, "MMK", request.PaymentMode, order.order_number), token);
            if (response.IsSuccessStatusCode)
            {
                var payment = await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken: token);
                paymentTransaction.payment_reference = payment?.PaymentReference;
                paymentTransaction.payment_status = payment?.Status ?? "Unknown";
                paymentTransaction.modify_date = DateTime.UtcNow;
            }
            else
            {
                paymentTransaction.payment_status = "Unknown";
            }
        }
        catch (HttpRequestException)
        {
            paymentTransaction.payment_status = "Unknown";
        }
        catch (TaskCanceledException) when (!token.IsCancellationRequested)
        {
            paymentTransaction.payment_status = "Unknown";
        }
        await _dbContext.SaveChangesAsync(token);
        return Result<OrderResponse>.Success(Map(order, paymentTransaction));
    }

    public async Task<Result<OrderResponse>> GetOrderAsync(Guid orderNumber, CancellationToken token)
    {
        var order = await _dbContext.tbl_orders.Include(x => x.tbl_payment_transactions).SingleOrDefaultAsync(x => x.order_number == orderNumber && !x.delete_flag, token);
        if (order is null)
        {
            return Result<OrderResponse>.Failure("not_found", "Order was not found.");
        }

        var paymentTransaction = order.tbl_payment_transactions.OrderByDescending(x => x.attempt_number).First();
        if (order.order_status == "PendingPayment" && paymentTransaction.payment_reference.HasValue)
        {
            try
            {
                var payment = await _httpClientFactory.CreateClient("PaymentApi").GetFromJsonAsync<PaymentResponse>($"api/payments/{paymentTransaction.payment_reference}", token);
                paymentTransaction.last_polled_date = DateTime.UtcNow;
                if (payment is not null)
                {
                    paymentTransaction.payment_status = payment.Status;
                    paymentTransaction.failure_reason = payment.FailureReason;
                    paymentTransaction.modify_date = DateTime.UtcNow;
                    if (payment.Status == "Succeeded")
                    {
                        order.order_status = "Paid";
                        paymentTransaction.completed_date = DateTime.UtcNow;
                    }
                    else if (payment.Status == "Failed")
                    {
                        order.order_status = "PaymentFailed";
                        paymentTransaction.completed_date = DateTime.UtcNow;
                    }

                    order.modify_date = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync(token);
                }
            }
            catch (HttpRequestException)
            {
                paymentTransaction.payment_status = "Unknown";
                paymentTransaction.last_polled_date = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(token);
            }
            catch (TaskCanceledException) when (!token.IsCancellationRequested)
            {
                paymentTransaction.payment_status = "Unknown";
            }
        }

        return Result<OrderResponse>.Success(Map(order, paymentTransaction));
    }

    private static OrderResponse Map(tbl_order order, tbl_payment_transaction payment)
    {
        return new OrderResponse(
            order.order_number,
            order.total_amount,
            order.order_status,
            payment.payment_status,
            payment.payment_reference,
            payment.failure_reason);
    }
}
