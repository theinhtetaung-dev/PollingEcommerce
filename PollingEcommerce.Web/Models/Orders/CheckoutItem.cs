namespace PollingEcommerce.Domain.Models.Orders;

public sealed class CheckoutItem
{
    public long ProductId { get; set; }

    public int Quantity { get; set; }
}
