using BuildingBlocks.Abstractions.Messages;

namespace Contracts.Messages.ProductCreated;

public sealed record ProductCreatedV1(
    Guid Id,
    string Name,
    string Category,
    string Description,
    decimal Price,
    DateTime CreatedAtUtc
) : IIntegrationEvent;
