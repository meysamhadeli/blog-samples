namespace Order.Orders.Models;

public class OrderItem
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }

    public Guid OrderId { get; init; }
    public Order Order { get; init; }
}
