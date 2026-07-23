using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.GettingProduct.v1;

internal static class GetProductEndpoint
{
    internal static RouteHandlerBuilder MapGetProductEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/products/{id:guid}", Handle).WithName("GetProduct");

    private static async Task<IResult> Handle(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductQuery(id), cancellationToken);
        return product is null ? Results.NotFound() : Results.Ok(product);
    }
}
