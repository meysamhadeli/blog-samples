using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Integration.MassTransit.Abstractions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BuildingBlocks.Integration.MassTransit;

internal sealed class DurableInternalCommandBus<TDbContext>(
    TDbContext dbContext, DurableCommandProcessorOptions options) : IInternalCommandBus
    where TDbContext : DbContext
{
    public async Task EnqueueAsync<T>(T command, CancellationToken cancellationToken = default)
        where T : class, IInternalCommand
    {
        var json = JsonConvert.SerializeObject(command,
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        var entity = new DurableMessage
        {
            TypeName = typeof(T).AssemblyQualifiedName!,
            Payload = json,
            Status = DurableMessageStatus.Pending,
        };
        dbContext.Set<DurableMessage>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
