using ECommerce.Services.Shared.Contracts.IntegrationEvents;
using ECommerce.Services.Shared.Contracts.MessageEnvelope;
using MassTransit;
using MediatR;

namespace ECommerce.Services.Orders.Products.Features.ConsumingProductCreated.v1;

public sealed class ProductCreatedConsumer(IMediator mediator)
    : IConsumer<MessageEnvelope<ProductCreatedV1>>
{
    public async Task Consume(ConsumeContext<MessageEnvelope<ProductCreatedV1>> context)
    {
        var message = context.Message.Message;
        await mediator.Publish(new ProductCreatedNotification(
            message.ProductId, message.Name, message.Price, message.Stock, message.CreatedAtUtc),
            context.CancellationToken);
    }
}
