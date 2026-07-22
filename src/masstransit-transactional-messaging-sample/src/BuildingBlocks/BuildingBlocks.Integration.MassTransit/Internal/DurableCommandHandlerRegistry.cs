using System.Collections.Concurrent;
using BuildingBlocks.Abstractions.Messages;

namespace BuildingBlocks.Integration.MassTransit;

internal static class DurableCommandHandlerRegistry
{
    private static readonly ConcurrentDictionary<Type, Delegate> Handlers = new();

    internal static void Register<T>(Func<T, IServiceProvider, CancellationToken, Task> handler)
        where T : class, IInternalCommand
        => Handlers[typeof(T)] = handler;

    internal static bool TryGet(Type type, out Delegate? handler) => Handlers.TryGetValue(type, out handler);
}
