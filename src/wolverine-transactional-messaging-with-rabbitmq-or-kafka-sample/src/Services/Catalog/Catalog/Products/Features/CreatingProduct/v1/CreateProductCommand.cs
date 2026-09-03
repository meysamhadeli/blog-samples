using BuildingBlocks.Integration.Wolverine.Abstractions;
using Catalog.Products.Models;
using Catalog.Shared.Data;
using Contracts.Messages.MessageEnvelope;
using Contracts.Messages.ProductCreated;
using Contracts.Messages.ProjectProductReadModel;
using MediatR;

namespace Catalog.Products.Features.CreatingProduct.v1;

public sealed record CreateProductCommand(
    string Name,
    decimal Price,
    int Stock,
    string? Category = null,
    string? Description = null) : IRequest<CreateProductResponse>;

internal sealed class CreateProductHandler(
    CatalogsDbContext dbContext,
    IEventBus eventBus,
    IInternalCommandBus internalCommandBus)
    : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price, request.Stock);
        dbContext.Products.Add(product);

        var integrationEvent = MessageEnvelope.Create(new ProductCreatedV1(
            product.Id,
            product.Name,
            request.Category ?? string.Empty,
            request.Description ?? string.Empty,
            product.Price,
            product.CreatedAtUtc));

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await internalCommandBus.EnqueueAsync(new ProjectProductReadModel(
            product.Id,
            product.Name,
            request.Category ?? string.Empty,
            request.Description ?? string.Empty,
            product.Price), cancellationToken);

        return new CreateProductResponse(product.Id, product.Name, product.Price, product.Stock);
    }
}
