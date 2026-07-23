using BuildingBlocks.Integration.MassTransit.Abstractions;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using ECommerce.Services.Shared.Contracts.IntegrationEvents;
using ECommerce.Services.Shared.Contracts.InternalCommands;
using ECommerce.Services.Shared.Contracts.MessageEnvelope;
using MediatR;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;

internal sealed record CreateProductCommand(string Name, decimal Price, int Stock)
    : IRequest<CreateProductResponse>;

internal sealed class CreateProductHandler(CatalogsDbContext dbContext, IEventBus eventBus, IMediator mediator)
    : IRequestHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<CreateProductResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(command.Name, command.Price, command.Stock);
        dbContext.Products.Add(product);

        var integrationEvent = MessageEnvelope.Create(new ProductCreatedV1(
            product.Id, product.Name, product.Price, product.Stock, product.CreatedAtUtc));

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Project read model via MediatR
        await mediator.Send(new ProjectProductReadModel(
            product.Id, product.Name, product.Price, product.Stock, product.CreatedAtUtc), cancellationToken);

        return new CreateProductResponse(product.Id, product.Name, product.Price, product.Stock);
    }
}

internal sealed record CreateProductRequest(string Name, decimal Price, int Stock);
internal sealed record CreateProductResponse(Guid Id, string Name, decimal Price, int Stock);
