using BuildingBlocks.Abstractions.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BuildingBlocks.Integration.MassTransit;

internal sealed class DurableCommandProcessor<TDbContext>(
    IServiceScopeFactory scopeFactory, DurableCommandProcessorOptions options,
    ILogger<DurableCommandProcessor<TDbContext>> logger) : BackgroundService
    where TDbContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DurableCommandProcessor started. Polling every {Interval}ms, batch size {Batch}",
            options.PollingInterval.TotalMilliseconds, options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await ProcessPendingCommandsAsync(stoppingToken); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            { logger.LogError(ex, "DurableCommandProcessor polling cycle failed"); }
            await Task.Delay(options.PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingCommandsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await ReclaimStaleCommandsAsync(dbContext, cancellationToken);

        var commands = await dbContext.Set<DurableMessage>()
            .Where(c => c.Status == DurableMessageStatus.Pending)
            .OrderBy(c => c.EnqueuedAtUtc).Take(options.BatchSize)
            .ToListAsync(cancellationToken);

        if (commands.Count == 0) return;

        foreach (var command in commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            command.Status = DurableMessageStatus.Processing;
            command.LastAttemptAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            try
            {
                await DispatchCommandAsync(command, scope.ServiceProvider, cancellationToken);
                command.Status = DurableMessageStatus.Completed;
                command.CompletedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
                logger.LogDebug("Command {Id} ({Type}) completed", command.Id, command.TypeName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                command.RetryCount++;
                command.LastError = ex.ToString();
                if (command.RetryCount >= options.MaxRetries)
                {
                    command.Status = DurableMessageStatus.Failed;
                    logger.LogError(ex, "Command {Id} ({Type}) failed after {Retries} retries",
                        command.Id, command.TypeName, options.MaxRetries);
                }
                else
                {
                    command.Status = DurableMessageStatus.Pending;
                    logger.LogWarning(ex, "Command {Id} ({Type}) failed (attempt {Retries}/{Max})",
                        command.Id, command.TypeName, command.RetryCount, options.MaxRetries);
                }
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private async Task ReclaimStaleCommandsAsync(TDbContext dbContext, CancellationToken ct)
    {
        var threshold = DateTime.UtcNow.Add(-options.StaleProcessingThreshold);
        var stale = await dbContext.Set<DurableMessage>()
            .Where(c => c.Status == DurableMessageStatus.Processing
                        && c.LastAttemptAtUtc != null
                        && c.LastAttemptAtUtc < threshold)
            .ToListAsync(ct);

        foreach (var cmd in stale)
        {
            cmd.Status = DurableMessageStatus.Pending;
            logger.LogWarning("Reclaiming stale command {Id} ({Type})", cmd.Id, cmd.TypeName);
        }

        if (stale.Count > 0)
            await dbContext.SaveChangesAsync(ct);
    }

    private async Task DispatchCommandAsync(DurableMessage message, IServiceProvider sp, CancellationToken ct)
    {
        var type = Type.GetType(message.TypeName)
            ?? throw new InvalidOperationException($"Cannot resolve type '{message.TypeName}'.");

        if (!DurableCommandHandlerRegistry.TryGet(type, out var handler))
            throw new InvalidOperationException($"No handler registered for '{type.Name}'.");

        var deserialized = JsonConvert.DeserializeObject(message.Payload, type,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

        if (deserialized is not IInternalCommand command)
            throw new InvalidOperationException($"Deserialized object is not an IInternalCommand.");

        await ((Task)handler.DynamicInvoke(command, sp, ct)!);
    }
}
