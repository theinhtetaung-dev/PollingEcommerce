namespace PollingEcommerce.MVC.Models;
public record ProductViewModel(long Id, string Name, string? Description, decimal Price, int Stock, int CategoryId, string Category);
public record CheckoutItemViewModel(long ProductId, int Quantity);
public sealed class CheckoutForm { public List<CheckoutItemViewModel> Items { get; set; } = []; public string PaymentMode { get; set; } = "Success"; }
public sealed class OrderViewModel { public Guid OrderNumber { get; set; } public decimal TotalAmount { get; set; } public string OrderStatus { get; set; } = "PendingPayment"; public string PaymentStatus { get; set; } = "Pending"; public Guid? PaymentReference { get; set; } public string? FailureReason { get; set; } }
