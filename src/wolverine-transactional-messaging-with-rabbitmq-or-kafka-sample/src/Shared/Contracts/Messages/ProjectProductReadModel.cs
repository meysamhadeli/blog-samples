using BuildingBlocks.Abstractions.Messages;
using MediatR;

namespace Contracts.Messages.ProjectProductReadModel;

public sealed record ProjectProductReadModel(
    Guid ProductId,
    string Name,
    string Category,
    string Description,
    decimal Price
) : IInternalCommand, IRequest;
