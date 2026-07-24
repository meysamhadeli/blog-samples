namespace ef.core.json.column.Models.ValueObjects;

public record SupplierInformation
{
    public string Name { get; set; }
    public Address Address { get; set; }
}