using BuildingBlocks.Integration.Wolverine;
using BuildingBlocks.Integration.Wolverine.Configuration;
using Catalog.Products.Features.CreatingProduct.v1;
using Catalog.Products.Features.GettingProduct.v1;
using Catalog.Shared.Extensions.HostApplicationBuilderExtensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog;

public static class CatalogModule
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddCatalogsStorage();

        var transport = builder.Configuration.GetMessagingTransport();

        // Register MediatR handlers (replaces Wolverine handler discovery
        // for internal commands like ProjectProductReadModel).
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CatalogModule).Assembly));

        // Single unified entry point for all Wolverine + messaging config.
        // Wolverine is used for transport only (RabbitMQ publish/subscribe).
        builder.AddTransactionalWolverine(transport, cfg =>
        {
            // Auto-discover integration events and wire RabbitMQ publishing.
            // Module prefix derived from assembly name: "Catalog" → "catalog"
            cfg.ScanIntegrationEvents(typeof(CatalogModule).Assembly);
        });
    }

    public static void MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/catalogs");
        group.MapCreateProductEndpoint();
        group.MapGetProductEndpoint();
    }
}
