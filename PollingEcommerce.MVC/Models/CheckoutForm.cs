namespace PollingEcommerce.MVC.Models;

public sealed class CheckoutForm
{
    public List<CheckoutItemViewModel> Items { get; set; } = [];

    public string PaymentMode { get; set; } = "Success";
}
