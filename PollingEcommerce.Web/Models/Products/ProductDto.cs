namespace PollingEcommerce.Domain.Models.Products;

public sealed class ProductDto
{
    public ProductDto(long id, string name, string? description, decimal price, int stock, int categoryId, string category)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        Category = category;
    }

    public long Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public decimal Price { get; }
    public int Stock { get; }
    public int CategoryId { get; }
    public string Category { get; }
}
