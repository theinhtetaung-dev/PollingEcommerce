namespace PollingEcommerce.MVC.Models.Store;

public sealed class ProductViewModel
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
}
