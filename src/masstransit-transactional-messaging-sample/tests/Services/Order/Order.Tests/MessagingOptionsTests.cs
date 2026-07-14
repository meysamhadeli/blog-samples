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
        var options = new MessagingOptions { Transport = MessagingTransport.Normalize(transport) };

        Assert.Equal(transport, options.Transport);
    }

    [Theory]
    [InlineData("rabbitmq", MessagingTransport.RabbitMq)]
    [InlineData("kafka", MessagingTransport.Kafka)]
    [InlineData(" Kafka ", MessagingTransport.Kafka)]
    public void order_api_should_normalize_supported_transport_values(string transport, string expected)
    {
        Assert.Equal(expected, MessagingTransport.Normalize(transport));
    }

    [Fact]
    public void order_api_should_reject_unsupported_transport_values()
    {
        Assert.Throws<InvalidOperationException>(() => MessagingTransport.Normalize("sql"));
    }
}