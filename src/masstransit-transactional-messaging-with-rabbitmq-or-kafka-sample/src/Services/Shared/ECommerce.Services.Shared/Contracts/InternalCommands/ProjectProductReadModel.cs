using BuildingBlocks.Abstractions.Messages;

namespace ECommerce.Services.Shared.Contracts.InternalCommands;

public sealed record ProjectProductReadModel(
    Guid ProductId, string Name, decimal Price, int Stock, DateTime CreatedAtUtc
) : IInternalCommand;
