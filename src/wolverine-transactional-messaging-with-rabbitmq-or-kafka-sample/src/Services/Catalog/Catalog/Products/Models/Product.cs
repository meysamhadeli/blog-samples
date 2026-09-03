namespace Catalog.Products.Models;

public class Product
{
    private Product() { }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Product Create(string name, decimal price, int stock) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Price = price,
        Stock = stock,
        CreatedAtUtc = DateTime.UtcNow,
    };
}
