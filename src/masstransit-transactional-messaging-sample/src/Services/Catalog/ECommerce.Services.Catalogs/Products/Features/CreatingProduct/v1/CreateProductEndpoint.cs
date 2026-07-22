using BuildingBlocks.Integration.MassTransit.Abstractions;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using ECommerce.Services.Shared.Contracts.IntegrationEvents;
using ECommerce.Services.Shared.Contracts.InternalCommands;
using ECommerce.Services.Shared.Contracts.MessageEnvelope;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;

internal static class CreateProductEndpoint
{
    internal static RouteHandlerBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/products", Handle).WithName("CreateProduct");

    private static async Task<CreatedAtRoute<CreateProductResponse>> Handle(
        [FromBody] CreateProductRequest request,
        CatalogsDbContext dbContext, IEventBus eventBus,
        IInternalCommandBus internalCommandBus, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price, request.Stock);
        dbContext.Products.Add(product);

        var integrationEvent = MessageEnvelope.Create(new ProductCreatedV1(
            product.Id, product.Name, product.Price, product.Stock, product.CreatedAtUtc));

        // Publish to MassTransit bus (goes through EF outbox)
        await eventBus.PublishAsync(integrationEvent, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Enqueue internal command for local read-model projection
        await internalCommandBus.EnqueueAsync(new ProjectProductReadModel(
            product.Id, product.Name, product.Price, product.Stock, product.CreatedAtUtc), cancellationToken);

        return TypedResults.CreatedAtRoute(
            new CreateProductResponse(product.Id, product.Name, product.Price, product.Stock),
            "CreateProduct", new { id = product.Id });
    }
}

internal sealed record CreateProductRequest(string Name, decimal Price, int Stock);
internal sealed record CreateProductResponse(Guid Id, string Name, decimal Price, int Stock);
