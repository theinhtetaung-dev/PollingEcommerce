namespace PollingEcommerce.MVC.Models;

public sealed class OrderViewModel
{
    public Guid OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = "PendingPayment";
    public string PaymentStatus { get; set; } = "Pending";
    public Guid? PaymentReference { get; set; }
    public string? FailureReason { get; set; }
}
