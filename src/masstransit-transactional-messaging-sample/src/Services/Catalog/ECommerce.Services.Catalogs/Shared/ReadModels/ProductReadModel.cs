using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Services.Catalogs.Shared.ReadModels;

public sealed record ProductReadModel(
    [property: BsonGuidRepresentation(GuidRepresentation.Standard)] Guid Id,
    string Name, decimal Price, int Stock, DateTime SyncedAt
);
