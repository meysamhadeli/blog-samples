using Contracts.Messages.MessageEnvelope;
using Contracts.Messages.ProductCreated;
using MediatR;

namespace Order.Products.Features.ConsumingProductCreated.v1;

/// <summary>
/// Thin Wolverine consumer — receives messages from RabbitMQ (discovered by
/// Wolverine convention), then delegates business logic to the MediatR handler.
/// </summary>
public sealed class ProductCreatedConsumer(IMediator mediator)
{
    public async Task Consume(MessageEnvelope<ProductCreatedV1> envelope)
        => await mediator.Publish(new ProductCreatedNotification(envelope));
}
