namespace ef.core.json.column.Models.ValueObjects;

public record Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}