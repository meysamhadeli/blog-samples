using Order.Models;

namespace Order.Data;

public sealed class OrderImportStore
{
    private readonly Dictionary<Guid, ImportedProduct> _products = new();

    public Task UpsertAsync(ImportedProduct product, CancellationToken cancellationToken = default)
    {
        _products[product.Id] = product;
        return Task.CompletedTask;
    }

    public Task<ImportedProduct?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public Task<IReadOnlyCollection<ImportedProduct>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyCollection<ImportedProduct>)_products.Values.ToList());
    }
}

public sealed class InboxStore
{
    private readonly HashSet<Guid> _processedMessageIds = new();

    public bool TryBegin(Guid messageId)
    {
        return _processedMessageIds.Add(messageId);
    }
}
