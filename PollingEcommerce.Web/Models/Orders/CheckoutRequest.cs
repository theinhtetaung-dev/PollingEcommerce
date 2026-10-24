namespace PollingEcommerce.Domain.Models.Orders;

public sealed class CheckoutRequest
{
    public long UserId { get; set; }

    public List<CheckoutItem> Items { get; set; } = [];

    public string PaymentMode { get; set; } = "Success";
}
