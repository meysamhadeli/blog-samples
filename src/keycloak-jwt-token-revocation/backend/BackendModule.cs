using backend.Products.Features.GettingProducts.v1;
using backend.Shared.Extensions.HostApplicationBuilderExtensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace backend;

public static class BackendModule
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(BackendModule).Assembly));
    }

    public static void MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/catalogs").RequireAuthorization();
        group.MapGetProductsEndpoint();
    }
}
