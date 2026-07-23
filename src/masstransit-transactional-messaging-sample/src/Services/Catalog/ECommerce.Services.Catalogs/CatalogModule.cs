using BuildingBlocks.Integration.MassTransit;
using BuildingBlocks.Integration.MassTransit.Configuration;
using BuildingBlocks.Integration.MassTransit.Options;
using ECommerce.Services.Catalogs.Products.Features.CreatingProduct.v1;
using ECommerce.Services.Catalogs.Products.Features.GettingProduct.v1;
using ECommerce.Services.Catalogs.Shared.Data;
using ECommerce.Services.Catalogs.Shared.Extensions.HostApplicationBuilderExtensions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Services.Catalogs;

public static class CatalogModule
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
            Bus = new MassTransitBusOptions { UseBusOutbox = true },
        };

        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CatalogModule).Assembly));

        // Single unified entry point for all MassTransit config
        builder.Services.AddTransactionalMassTransit<CatalogsDbContext>(options, transport, cfg =>
        {
            cfg.ScanIntegrationEvents(typeof(CatalogModule).Assembly);
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
