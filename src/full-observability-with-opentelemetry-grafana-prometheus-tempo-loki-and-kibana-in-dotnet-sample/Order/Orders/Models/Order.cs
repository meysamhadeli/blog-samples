namespace Order.Orders.Models;

public class Order
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; }
    public string ShippingAddress { get; init; }
    public decimal TotalPrice { get; init; }
    public string Status { get; init; } // Pending, Shipped, Delivered, Cancelled
    public DateTime CreatedAt { get; init; }

    public ICollection<OrderItem> Items { get; init; } = new List<OrderItem>();
}
