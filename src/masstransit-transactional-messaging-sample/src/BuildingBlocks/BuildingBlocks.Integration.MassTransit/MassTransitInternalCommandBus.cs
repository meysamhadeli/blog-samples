using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Integration.MassTransit.Abstractions;
using MediatR;

namespace BuildingBlocks.Integration.MassTransit;

internal sealed class MassTransitInternalCommandBus(IMediator mediator) : IInternalCommandBus
{
    public async Task EnqueueAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : IInternalCommand
        => await mediator.Send(command, cancellationToken);
}
