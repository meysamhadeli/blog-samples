using Catalog;
using Catalog.Data;
using Contracts;
using Order;
using Order.Data;

namespace Tests.Shared;

public sealed class SampleFlow
{
    public async Task<FlowResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var catalogService = new CatalogService(new CatalogWriteStore(), new CatalogReadStore());
        var orderService = new OrderImportService(new OrderImportStore(), new InboxStore());

        var createResult = await catalogService.CreateProductAsync(
            new CreateProductRequest("Mechanical Keyboard", 149.99m, 10, "catalog-flow"),
            cancellationToken);

        var imported = await orderService.ImportAsync(createResult.IntegrationEvent, cancellationToken);
        var writeModel = await catalogService.GetProductAsync(createResult.ProductId, cancellationToken);
        var readModel = await catalogService.GetReadModelAsync(createResult.ProductId, cancellationToken);
        var orderProduct = await orderService.GetProductAsync(createResult.ProductId, cancellationToken);

        return new FlowResult(createResult, imported, writeModel, readModel, orderProduct);
    }
}

public sealed record FlowResult(
    CreateProductResult CreateResult,
    bool Imported,
    object? WriteModel,
    object? ReadModel,
    object? OrderProduct);
