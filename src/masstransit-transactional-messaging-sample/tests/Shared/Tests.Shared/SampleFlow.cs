using Catalog;
using Catalog.Data;
using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Order;
using Order.Data;

namespace Tests.Shared;

public sealed class SampleFlow
{
    public async Task<FlowResult> RunAsync(string transport = "rabbitmq", CancellationToken cancellationToken = default)
    {
        await using var provider = new ServiceCollection()
            .AddSingleton<CatalogWriteStore>()
            .AddSingleton<CatalogReadStore>()
            .AddSingleton<OrderImportStore>()
            .AddSingleton<InboxStore>()
            .AddMassTransit(x =>
            {
                x.AddConsumer<OrderImportService>();
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            })
            .AddScoped<CatalogService>()
            .BuildServiceProvider(true);

        var busControl = provider.GetRequiredService<IBusControl>();
        await busControl.StartAsync(cancellationToken);

        try
        {
            await using var scope = provider.CreateAsyncScope();
            var catalogService = scope.ServiceProvider.GetRequiredService<CatalogService>();

            var createResult = await catalogService.CreateProductAsync(
                new CreateProductRequest("Mechanical Keyboard", 149.99m, 10, $"catalog-flow-{transport}"),
                cancellationToken);

            await Task.Delay(200, cancellationToken);

            await using var orderScope = provider.CreateAsyncScope();
            var orderService = orderScope.ServiceProvider.GetRequiredService<OrderImportService>();
            var orderProduct = await orderService.GetProductAsync(createResult.ProductId, cancellationToken);
            var writeModel = await catalogService.GetProductAsync(createResult.ProductId, cancellationToken);
            var readModel = await catalogService.GetReadModelAsync(createResult.ProductId, cancellationToken);

            return new FlowResult(transport, createResult, orderProduct is not null, writeModel, readModel, orderProduct);
        }
        finally
        {
            await busControl.StopAsync(cancellationToken);
        }
    }
}

public sealed record FlowResult(
    string Transport,
    CreateProductResult CreateResult,
    bool Imported,
    object? WriteModel,
    object? ReadModel,
    object? OrderProduct);
