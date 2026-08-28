using AgentTools;

namespace AgentTools.UnitTests;

public sealed class SupportDataTests
{
    [Fact]
    public void GetOrderStatus_ReturnsKnownOrder()
    {
        var result = new OrderStore().GetOrderStatus("ord-1001");

        Assert.Equal("ORD-1001", result.OrderId);
        Assert.Equal("Payment review", result.Status);
        Assert.Equal("Pending charge", result.PaymentStatus);
    }

    [Fact]
    public void GetOrderStatus_ReturnsUnknownForMissingOrder()
    {
        var result = new OrderStore().GetOrderStatus("ORD-9999");

        Assert.Equal("Unknown", result.Status);
        Assert.Equal("Unknown", result.PaymentStatus);
    }

    [Fact]
    public void Search_ReturnsPendingChargeGuidance()
    {
        var result = new SupportKnowledgeBase().Search("pending charge");

        Assert.Contains("Do not ask the customer to retry repeatedly", result);
        Assert.Contains("bank statement", result);
    }

    [Fact]
    public void Search_ReturnsNoMatchForUnrelatedQuery()
    {
        var result = new SupportKnowledgeBase().Search("shipping address");

        Assert.Equal("No matching support guidance was found.", result);
    }
}
