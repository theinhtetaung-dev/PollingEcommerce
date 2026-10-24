namespace PollingPayment.Domain.Models.Payment;

public sealed class CreatePaymentRequest
{
    public Guid MerchantOrderNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string TestMode { get; set; } = string.Empty;
    public Guid IdempotencyKey { get; set; }
}
