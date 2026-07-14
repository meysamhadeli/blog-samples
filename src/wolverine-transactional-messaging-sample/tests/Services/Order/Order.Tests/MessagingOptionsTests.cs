using Contracts;
using Xunit;

namespace Order.Tests;

public sealed class MessagingOptionsTests
{
    public static TheoryData<string> SupportedTransports => new()
    {
        "rabbitmq",
        "kafka"
    };

    [Fact]
    public void order_api_should_default_transport_to_rabbitmq()
    {
        var options = new MessagingOptions();

        Assert.Equal("rabbitmq", options.Transport);
    }

    [Theory]
    [MemberData(nameof(SupportedTransports))]
    public void order_api_should_allow_supported_transport_values(string transport)
    {
        var options = new MessagingOptions { Transport = transport };

        Assert.Equal(transport, options.Transport);
    }
}