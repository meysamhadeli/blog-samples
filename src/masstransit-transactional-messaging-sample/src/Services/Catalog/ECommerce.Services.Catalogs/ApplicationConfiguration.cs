using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Integration.MassTransit.Configuration;
using BuildingBlocks.Integration.MassTransit.Options;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;
using ECommerce.Services.Catalogs.Products.Features.GettingProduct.v1;
using ECommerce.Services.Catalogs.Shared.Data;
using ECommerce.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Services.Catalogs;

public static class ApplicationConfiguration
{
    public const string CatalogModulePrefixUri = "/api/v1/catalogs";

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.AddCatalogsStorage();

        var transport = builder.Configuration.GetMessagingTransport();
        var connectionString = builder.Configuration.GetConnectionString("catalogsdb")
            ?? throw new InvalidOperationException("Missing connection string 'catalogsdb'.");

        var options = new MassTransitOptions
        {
            DurableStorageConnectionString = connectionString,
            RabbitMqConnectionString = builder.Configuration.GetConnectionString("rabbitmq"),
            KafkaConnectionString = builder.Configuration.GetConnectionString("kafka"),
            Bus = new MassTransitBusOptions { UseBusOutbox = true, UsePostCommitMediator = true },
        };

        // Single unified entry point for all MassTransit + durable local queue config
        builder.Services.AddTransactionalMassTransit<CatalogsDbContext>(options, transport, cfg =>
        {
            // Auto-discover integration events via type-scanning convention
            cfg.ScanIntegrationEvents(typeof(ApplicationConfiguration).Assembly);

            // Durable internal commands (replaces mediator-based IInternalCommandBus)
            cfg.UseDurableInternalCommands();
            cfg.ScanDurableHandlers(typeof(ApplicationConfiguration).Assembly);
        }, builder.Environment);

        return builder;
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(CatalogModulePrefixUri);
        group.MapCreateProductEndpoint();
        group.MapGetProductEndpoint();
        return endpoints;
    }
}
