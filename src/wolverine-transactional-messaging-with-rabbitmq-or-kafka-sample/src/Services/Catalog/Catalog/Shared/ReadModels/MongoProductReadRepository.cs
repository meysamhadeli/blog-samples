using Catalog.Products.Models;
using MongoDB.Driver;

namespace Catalog.Shared.ReadModels;

public interface IProductReadRepository
{
    Task UpsertAsync(ProductReadModel model, CancellationToken cancellationToken);
}

public sealed class MongoProductReadRepository(IMongoDatabase database) : IProductReadRepository
{
    private readonly IMongoCollection<ProductReadModel> _collection =
        database.GetCollection<ProductReadModel>("product-read-models");

    public Task UpsertAsync(ProductReadModel model, CancellationToken cancellationToken)
        => _collection.ReplaceOneAsync(
            x => x.Id == model.Id, model,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
}
