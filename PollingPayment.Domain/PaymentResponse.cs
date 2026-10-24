namespace PollingPayment.Domain;

public sealed class PaymentResponse
{
    public PaymentResponse(Guid paymentReference, Guid merchantOrderNumber, decimal amount, string currencyCode, string status, string? failureReason)
    {
        PaymentReference = paymentReference;
        MerchantOrderNumber = merchantOrderNumber;
        Amount = amount;
        CurrencyCode = currencyCode;
        Status = status;
        FailureReason = failureReason;
    }

    public Guid PaymentReference { get; }
    public Guid MerchantOrderNumber { get; }
    public decimal Amount { get; }
    public string CurrencyCode { get; }
    public string Status { get; }
    public string? FailureReason { get; }
}
