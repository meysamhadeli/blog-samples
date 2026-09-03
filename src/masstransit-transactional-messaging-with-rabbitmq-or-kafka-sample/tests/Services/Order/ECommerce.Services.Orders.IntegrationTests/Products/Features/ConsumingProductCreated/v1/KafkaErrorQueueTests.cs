using ECommerce.Services.Orders;
using ECommerce.Services.Orders.Products.Features.ConsumingProductCreated.v1;
using ECommerce.Services.Shared.Contracts.IntegrationEvents;
using ECommerce.Services.Shared.Contracts.MessageEnvelope;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tests.Shared.Fixtures;
using Xunit;

namespace ECommerce.Services.Orders.IntegrationTests.Products.Features.ConsumingProductCreated.v1;

public class KafkaErrorQueueTests(OrdersSharedFixture sharedFixture)
    : OrdersIntegrationTestBase(sharedFixture)
{
    protected override string MessagingTransport => "kafka";

    [Fact]
    public async Task FaultyKafkaMessage_ShouldBeHandled_ByKafkaTransportPath()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var _ = Factory.CreateClient();

        var envelope = MessageEnvelope.Create(
            new ProductCreatedV1(
                Guid.NewGuid(),
                "faulty-product-created",
                9.99m,
                1,
                DateTime.UtcNow
            )
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InvokeProductCreatedConsumerAsync(envelope, cancellationToken)
        );

        Assert.Equal(
            "Intentional consumer failure for retry and dead-letter tests.",
            exception.Message
        );
    }

    [Fact]
    public async Task FaultyKafkaMessage_ShouldBeForwarded_ToDeadLetterTopic()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var _ = Factory.CreateClient();

        var envelope = MessageEnvelope.Create(
            new ProductCreatedV1(
                Guid.NewGuid(),
                "faulty-product-created",
                9.99m,
                1,
                DateTime.UtcNow
            )
        );

        var deadLetterProducer = new Mock<ITopicProducer<MessageEnvelope<ProductCreatedV1>>>();
        var consumer = new KafkaDeadLetterForwardingConsumer(deadLetterProducer.Object);

        var faultMock = new Mock<Fault<MessageEnvelope<ProductCreatedV1>>>();
        faultMock.Setup(x => x.Message).Returns(envelope);
        var contextMock = new Mock<ConsumeContext<Fault<MessageEnvelope<ProductCreatedV1>>>>();
        contextMock.Setup(x => x.Message).Returns(faultMock.Object);
        contextMock.Setup(x => x.CancellationToken).Returns(cancellationToken);

        await consumer.Consume(contextMock.Object);

        deadLetterProducer.Verify(x => x.Produce(envelope, cancellationToken), Times.Once);
    }

    private async Task InvokeProductCreatedConsumerAsync(
        MessageEnvelope<ProductCreatedV1> envelope,
        CancellationToken cancellationToken
    )
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<ProductCreatedConsumer>();
        var context = Mock.Of<ConsumeContext<MessageEnvelope<ProductCreatedV1>>>(x =>
            x.Message == envelope && x.CancellationToken == cancellationToken
        );

        await consumer.Consume(context);
    }
}
