using BuildingBlocks.Integration.MassTransit.Configuration;
using ECommerce.Services.Shared.Contracts.IntegrationEvents;
using ECommerce.Services.Shared.Contracts.MessageEnvelope;
using MassTransit;

namespace ECommerce.Services.Orders.Products.Features.ConsumingProductCreated.v1;

public sealed class KafkaDeadLetterForwardingConsumer(
    ITopicProducer<MessageEnvelope<ProductCreatedV1>> deadLetterProducer
) : IConsumer<Fault<MessageEnvelope<ProductCreatedV1>>>
{
    public Task Consume(ConsumeContext<Fault<MessageEnvelope<ProductCreatedV1>>> context)
        => deadLetterProducer.Produce(context.Message.Message, context.CancellationToken);
}
