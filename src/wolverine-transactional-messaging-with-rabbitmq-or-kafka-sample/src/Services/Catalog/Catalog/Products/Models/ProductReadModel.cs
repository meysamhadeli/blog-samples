using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Catalog.Products.Models;

public sealed record ProductReadModel(
    [property: BsonGuidRepresentation(GuidRepresentation.Standard)] Guid Id,
    string Name,
    string Category,
    string Description,
    decimal Price,
    DateTime SyncedAt
);
