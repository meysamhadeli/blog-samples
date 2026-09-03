using BuildingBlocks.Integration.Wolverine;
using BuildingBlocks.Integration.Wolverine.Configuration;
using Contracts.Messages.Constants;
using Contracts.Messages.ProductCreated;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Products.Features.ConsumingProductCreated.v1;
using Order.Shared.Extensions.HostApplicationBuilderExtensions;

namespace Order;

public static class OrderModule
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddOrdersStorage();

        var transport = builder.Configuration.GetMessagingTransport();

        // Register MediatR handlers (for integration event processing).
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(OrderModule).Assembly));

        // Single unified entry point for all Wolverine + messaging config.
        // Wolverine is used for transport only (RabbitMQ pub/sub).
        builder.AddTransactionalWolverine(transport, cfg =>
        {
            // Auto-discover integration events and wire RabbitMQ listening.
            // Prefix must match Catalog publisher's convention: "{prefix}-*"
            cfg.ListenToIntegrationEvents(MessagingConstants.ModulePrefixes.Catalog, typeof(ProductCreatedV1).Assembly);

            // Register thin Wolverine bridge consumers that forward to MediatR.
            cfg.ScanHandlers(typeof(OrderModule).Assembly);
        });
    }

    public static void MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/orders");
        group.MapReceiveProductCreatedEndpoint();
    }
}
