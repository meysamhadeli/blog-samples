using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace backend.Products.Features.GettingProducts.v1;

internal static class GetProductsEndpoint
{
    internal static RouteHandlerBuilder MapGetProductsEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/products", Handle).WithName("GetProducts");

    private static async Task<IResult> Handle(
        IMediator mediator,
        CancellationToken cancellationToken)
        => Results.Ok(await mediator.Send(new GetProductsQuery(), cancellationToken));
}
