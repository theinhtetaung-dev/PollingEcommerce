using Microsoft.EntityFrameworkCore;
using Polling.Shared;
using PollingPayment.DataAccess.Models;
using PollingPayment.DataAccess.AppDbContext;

namespace PollingPayment.Domain;

public sealed class PaymentService : IPaymentService
{
    private readonly PollingPaymentDbContext _dbContext;

    public PaymentService(PollingPaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaymentResponse>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken)
    {
        if (request.MerchantOrderNumber == Guid.Empty || request.IdempotencyKey == Guid.Empty || request.Amount <= 0)
        {
            return Result<PaymentResponse>.Failure("validation", "Order number, positive amount and idempotency key are required.");
        }

        if (!new[] { "Success", "Failed", "Delayed" }.Contains(request.TestMode))
        {
            return Result<PaymentResponse>.Failure("validation", "TestMode must be Success, Failed or Delayed.");
        }

        var existing = await _dbContext.tbl_payments.AsNoTracking().SingleOrDefaultAsync(x => x.idempotency_key == request.IdempotencyKey, cancellationToken);
        if (existing is not null)
        {
            if (existing.merchant_order_number != request.MerchantOrderNumber || existing.amount != request.Amount)
            {
                return Result<PaymentResponse>.Failure("conflict", "The idempotency key was already used for a different payment.");
            }

            return Result<PaymentResponse>.Success(Map(existing));
        }

        var now = DateTime.UtcNow;
        var payment = new tbl_payment
        {
            payment_reference = Guid.NewGuid(),
            merchant_order_number = request.MerchantOrderNumber,
            amount = request.Amount,
            currency_code = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "MMK" : request.CurrencyCode.ToUpperInvariant(),
            payment_status = "Pending",
            test_mode = request.TestMode,
            idempotency_key = request.IdempotencyKey,
            process_after = now.AddSeconds(request.TestMode == "Delayed" ? 15 : 3),
            add_date = now
        };
        _dbContext.tbl_payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _dbContext.tbl_payment_events.Add(new tbl_payment_event
        {
            payment_id = payment.payment_id,
            new_status = "Pending",
            event_description = "Payment accepted.",
            add_date = now
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<PaymentResponse>.Success(Map(payment));
    }

    public async Task<Result<PaymentResponse>> GetAsync(Guid reference, CancellationToken cancellationToken)
    {
        var payment = await _dbContext.tbl_payments.SingleOrDefaultAsync(x => x.payment_reference == reference && !x.delete_flag, cancellationToken);
        if (payment is null)
        {
            return Result<PaymentResponse>.Failure("not_found", "Payment was not found.");
        }

        if (payment.payment_status is "Pending" or "Processing" && payment.process_after <= DateTime.UtcNow)
        {
            var previous = payment.payment_status;
            payment.payment_status = payment.test_mode == "Failed" ? "Failed" : "Succeeded";
            payment.failure_reason = payment.payment_status == "Failed" ? "Simulated payment failure." : null;
            payment.completed_date = DateTime.UtcNow;
            payment.modify_date = DateTime.UtcNow;
            _dbContext.tbl_payment_events.Add(new tbl_payment_event
            {
                payment_id = payment.payment_id,
                previous_status = previous,
                new_status = payment.payment_status,
                event_description = "Simulation completed.",
                add_date = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result<PaymentResponse>.Success(Map(payment));
    }

    private static PaymentResponse Map(tbl_payment payment)
    {
        return new PaymentResponse(
            payment.payment_reference,
            payment.merchant_order_number,
            payment.amount,
            payment.currency_code,
            payment.payment_status,
            payment.failure_reason);
    }
}
