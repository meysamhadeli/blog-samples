using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Integration.MassTransit.Abstractions;
using MassTransit.Mediator;

namespace BuildingBlocks.Integration.MassTransit;

internal sealed class MassTransitInternalCommandBus(IMediator mediator) : IInternalCommandBus
{
    public Task EnqueueAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, IInternalCommand => mediator.Publish(command, cancellationToken);
}
