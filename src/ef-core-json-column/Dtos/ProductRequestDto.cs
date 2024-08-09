using ef.core.json.column.Models.ValueObjects;

namespace ef.core.json.column.Dtos;

public class ProductRequestDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public IEnumerable<SupplierInformationDto>? SupplierInformations { get; set; } = new List<SupplierInformationDto>();
}