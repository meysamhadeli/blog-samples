using Catalog.Data;
using Catalog.Models;
using Contracts;
using MassTransit;

namespace Catalog;

public sealed class CatalogService
{
    private readonly CatalogWriteStore _writeStore;
    private readonly CatalogReadStore _readStore;
    private readonly IPublishEndpoint _publishEndpoint;

    public CatalogService(CatalogWriteStore writeStore, CatalogReadStore readStore, IPublishEndpoint publishEndpoint)
    {
        _writeStore = writeStore;
        _readStore = readStore;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateProductResult> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Stock = request.Stock
        };

        await _writeStore.AddAsync(product, cancellationToken);

        var integrationEvent = MessageEnvelope<ProductCreatedV1>.Create(
            new ProductCreatedV1(product.Id, product.Name, product.Price, product.Stock),
            request.CorrelationId);

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        await ProjectReadModelAsync(new ProjectProductReadModel(product.Id), cancellationToken);

        return new CreateProductResult(product.Id, integrationEvent);
    }

    public async Task ProjectReadModelAsync(ProjectProductReadModel command, CancellationToken cancellationToken = default)
    {
        var product = await _writeStore.FindAsync(command.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Product '{command.ProductId}' not found.");

        await _readStore.UpsertAsync(new ProductReadModel
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            SyncedAt = DateTime.UtcNow
        }, cancellationToken);
    }

    public Task<Product?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _writeStore.FindAsync(id, cancellationToken);
    }

    public Task<ProductReadModel?> GetReadModelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _readStore.FindAsync(id, cancellationToken);
    }
}

public sealed record CreateProductRequest(string Name, decimal Price, int Stock, string? CorrelationId);

public sealed record CreateProductResult(Guid ProductId, MessageEnvelope<ProductCreatedV1> IntegrationEvent);
