using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.CreatingProduct.v1;

internal static class CreateProductEndpoint
{
    internal static RouteHandlerBuilder MapCreateProductEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/products", Handle).WithName("CreateProduct");

    private static async Task<CreatedAtRoute<CreateProductResponse>> Handle(
        [FromBody] CreateProductRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateProductCommand(
            request.Name, request.Price, request.Stock,
            request.Category, request.Description), cancellationToken);

        return TypedResults.CreatedAtRoute(result, "CreateProduct", new { id = result.Id });
    }
}

public sealed record CreateProductRequest(
    string Name, decimal Price, int Stock,
    string? Category = null, string? Description = null);

public sealed record CreateProductResponse(Guid Id, string Name, decimal Price, int Stock);
