namespace backend.Products.Models;

public sealed record Product(
    int Id,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    string Description);
