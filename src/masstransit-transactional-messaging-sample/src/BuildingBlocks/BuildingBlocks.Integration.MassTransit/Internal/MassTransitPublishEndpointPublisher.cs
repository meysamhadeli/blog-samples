using BuildingBlocks.Integration.MassTransit.Abstractions;
using MassTransit;

namespace BuildingBlocks.Integration.MassTransit;

internal sealed class MassTransitPublishEndpointPublisher(IPublishEndpoint publishEndpoint)
    : IMassTransitMessagePublisher
{
    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
        where TMessage : class => publishEndpoint.Publish(message, cancellationToken);
}
