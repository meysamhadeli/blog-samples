namespace Order.Models;

public sealed class ImportedProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid SourceMessageId { get; set; }
}
