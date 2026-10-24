namespace PollingEcommerce.Domain.Models.Payments;

internal sealed class PaymentCreateRequest
{
    public PaymentCreateRequest(Guid merchantOrderNumber, decimal amount, string currencyCode, string testMode, Guid idempotencyKey)
    {
        MerchantOrderNumber = merchantOrderNumber;
        Amount = amount;
        CurrencyCode = currencyCode;
        TestMode = testMode;
        IdempotencyKey = idempotencyKey;
    }

    public Guid MerchantOrderNumber { get; }
    public decimal Amount { get; }
    public string CurrencyCode { get; }
    public string TestMode { get; }
    public Guid IdempotencyKey { get; }
}
