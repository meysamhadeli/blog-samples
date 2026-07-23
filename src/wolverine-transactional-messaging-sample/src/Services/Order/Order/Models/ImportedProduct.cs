namespace Order.Products.Models;

public class ImportedProduct
{
    private ImportedProduct() { }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static ImportedProduct Create(Guid id, string name, decimal price, int stock, DateTime createdAtUtc)
        => new() { Id = id, Name = name, Price = price, Stock = stock, CreatedAtUtc = createdAtUtc };

    public void Update(string name, decimal price, int stock, DateTime createdAtUtc)
    {
        Name = name;
        Price = price;
        Stock = stock;
        CreatedAtUtc = createdAtUtc;
    }
}
