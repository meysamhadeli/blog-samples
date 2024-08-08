using ef.core.json.column.Models.ValueObjects;

namespace ef.core.json.column.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<SupplierInformation>? SupplierInformations { get; set; } = new List<SupplierInformation>();
}

