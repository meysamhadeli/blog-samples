using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Integration.MassTransit.Abstractions;

public interface IInternalCommandBus
{
    Task EnqueueAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, IInternalCommand;
}
