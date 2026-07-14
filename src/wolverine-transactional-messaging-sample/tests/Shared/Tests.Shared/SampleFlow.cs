using Catalog;
using Catalog.Data;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order;
using Order.Data;
using Wolverine;

namespace Tests.Shared;

public sealed class SampleFlow
{
    public async Task<FlowResult> RunAsync(string transport = "rabbitmq", CancellationToken cancellationToken = default)
    {
        using var host = await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<CatalogWriteStore>();
                services.AddSingleton<CatalogReadStore>();
                services.AddSingleton<OrderImportStore>();
                services.AddSingleton<InboxStore>();
                services.AddScoped<CatalogService>();
                services.AddScoped<OrderImportService>();
            })
            .UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(OrderImportService).Assembly);
                opts.PublishMessage<MessageEnvelope<ProductCreatedV1>>().ToLocalQueue("catalog-products-created");
                opts.LocalQueue("catalog-products-created");
            })
            .StartAsync(cancellationToken);

        await using var scope = host.Services.CreateAsyncScope();
        var catalogService = scope.ServiceProvider.GetRequiredService<CatalogService>();
        var orderService = scope.ServiceProvider.GetRequiredService<OrderImportService>();

        var createResult = await catalogService.CreateProductAsync(
            new CreateProductRequest("Mechanical Keyboard", 149.99m, 10, $"catalog-flow-{transport}"),
            cancellationToken);

        await Task.Delay(200, cancellationToken);

        var importedProduct = await orderService.GetProductAsync(createResult.ProductId, cancellationToken);
        var writeModel = await catalogService.GetProductAsync(createResult.ProductId, cancellationToken);
        var readModel = await catalogService.GetReadModelAsync(createResult.ProductId, cancellationToken);
        var orderProduct = importedProduct;

        await host.StopAsync(cancellationToken);

        return new FlowResult(transport, createResult, importedProduct is not null, writeModel, readModel, orderProduct);
    }
}

public sealed record FlowResult(
    string Transport,
    CreateProductResult CreateResult,
    bool Imported,
    object? WriteModel,
    object? ReadModel,
    object? OrderProduct);
