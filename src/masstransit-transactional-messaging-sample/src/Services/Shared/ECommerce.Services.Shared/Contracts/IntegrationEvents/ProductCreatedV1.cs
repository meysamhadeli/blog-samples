using BuildingBlocks.Abstractions.Messages;

namespace ECommerce.Services.Shared.Contracts.IntegrationEvents;

// Matches old sample's product fields: ProductId, Name, Price, Stock
public sealed record ProductCreatedV1(
    Guid ProductId, string Name, decimal Price, int Stock, DateTime CreatedAtUtc
) : IIntegrationEvent;
