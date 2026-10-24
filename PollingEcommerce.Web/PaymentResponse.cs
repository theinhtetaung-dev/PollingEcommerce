namespace PollingEcommerce.Domain;

internal sealed class PaymentResponse
{
    public Guid PaymentReference { get; set; }
    public Guid MerchantOrderNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FailureReason { get; set; }
}
