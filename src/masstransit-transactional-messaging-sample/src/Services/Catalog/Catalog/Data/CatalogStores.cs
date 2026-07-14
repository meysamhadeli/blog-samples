using Catalog.Models;

namespace Catalog.Data;

public sealed class CatalogWriteStore
{
    private readonly Dictionary<Guid, Product> _products = new();

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task<Product?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyCollection<Product>)_products.Values.ToList());
    }
}

public sealed class CatalogReadStore
{
    private readonly Dictionary<Guid, ProductReadModel> _products = new();

    public Task UpsertAsync(ProductReadModel product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task<ProductReadModel?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyCollection<ProductReadModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyCollection<ProductReadModel>)_products.Values.ToList());
    }
}
