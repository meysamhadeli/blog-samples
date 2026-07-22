using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProduct.v1;

internal static class GetProductEndpoint
{
    internal static RouteHandlerBuilder MapGetProductEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapGet("/products/{id:guid}", Handle).WithName("GetProduct");

    private static async Task<IResult> Handle(Guid id, CatalogsDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.SingleOrDefaultAsync(
            x => x.Id == id, cancellationToken);
        return product is null ? Results.NotFound() : Results.Ok(product);
    }
}
