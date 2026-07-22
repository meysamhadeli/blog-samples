using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Integration.MassTransit.Abstractions;

/// <summary>
/// Marker/handler interface for durable internal commands.
/// Implement this for each command type; the handler is auto-discovered
/// by <c>ScanDurableHandlers()</c> and resolved from DI at runtime.
/// </summary>
public interface IInternalCommandHandler<in TCommand>
    where TCommand : class, IInternalCommand
{
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}
