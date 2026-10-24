namespace PollingEcommerce.Domain;

public sealed class OrderResponse
{
    public OrderResponse(Guid orderNumber, decimal totalAmount, string orderStatus, string paymentStatus, Guid? paymentReference, string? failureReason)
    {
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
        OrderStatus = orderStatus;
        PaymentStatus = paymentStatus;
        PaymentReference = paymentReference;
        FailureReason = failureReason;
    }

    public Guid OrderNumber { get; }
    public decimal TotalAmount { get; }
    public string OrderStatus { get; }
    public string PaymentStatus { get; }
    public Guid? PaymentReference { get; }
    public string? FailureReason { get; }
}
