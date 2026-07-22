using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Integration.MassTransit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Integration.MassTransit;

/// <summary>
/// Scans assemblies for <see cref="IInternalCommandHandler{T}"/> implementations
/// and auto-registers them in both DI and the static <see cref="DurableCommandHandlerRegistry"/>.
/// </summary>
internal static class DurableHandlerScanner
{
    private static readonly Type HandlerInterface = typeof(IInternalCommandHandler<>);

    /// <summary>
    /// Scans the specified <paramref name="assemblies"/> for concrete
    /// <see cref="IInternalCommandHandler{T}"/> implementations, registers each
    /// handler type as a scoped service, and wires a lambda into the registry
    /// that resolves the handler from DI at dispatch time.
    /// </summary>
    internal static void ScanAndRegister(IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            foreach (var handlerType in assembly.GetTypes()
                         .Where(t => t is { IsAbstract: false, IsInterface: false }))
            {
                foreach (var iface in handlerType.GetInterfaces())
                {
                    if (iface is { IsGenericType: true }
                        && iface.GetGenericTypeDefinition() == HandlerInterface)
                    {
                        var commandType = iface.GetGenericArguments()[0];

                        var registerMethod = typeof(DurableHandlerScanner)
                            .GetMethod(nameof(RegisterFromContainer),
                                BindingFlags.Static | BindingFlags.NonPublic)
                            ?.MakeGenericMethod(commandType);

                        registerMethod?.Invoke(null, [services, handlerType]);
                    }
                }
            }
        }
    }

    private static void RegisterFromContainer<TCommand>(IServiceCollection services, Type handlerType)
        where TCommand : class, IInternalCommand
    {
        services.AddScoped(handlerType);
        DurableCommandHandlerRegistry.Register<TCommand>(async (command, sp, ct) =>
        {
            var handler = (IInternalCommandHandler<TCommand>)sp.GetRequiredService(handlerType);
            await handler.Handle(command, ct);
        });
    }
}
