using Xunit;
using Catalog.Models;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order;
using Order.Data;
using Tests.Shared;
using Wolverine;

namespace Catalog.Tests;

public sealed class CatalogIntegrationTests
{
    public static TheoryData<string> SupportedTransports => new()
    {
        "rabbitmq",
        "kafka"
    };

    [Theory]
    [MemberData(nameof(SupportedTransports))]
    public async Task create_product_should_persist_write_model_and_project_read_model_for_supported_brokers(string transport)
    {
        var flow = new SampleFlow();

        var result = await flow.RunAsync(transport);

        Assert.Equal(transport, result.Transport);
        Assert.True(result.Imported);
        Assert.NotNull(result.WriteModel);
        Assert.NotNull(result.ReadModel);
        Assert.NotNull(result.OrderProduct);
        Assert.IsType<Product>(result.WriteModel);
        Assert.IsType<ProductReadModel>(result.ReadModel);
    }

    [Theory]
    [MemberData(nameof(SupportedTransports))]
    public async Task create_product_should_publish_integration_event_that_order_handler_can_consume(string transport)
    {
        using var host = await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<Catalog.Data.CatalogWriteStore>();
                services.AddSingleton<Catalog.Data.CatalogReadStore>();
                services.AddSingleton<OrderImportStore>();
                services.AddSingleton<InboxStore>();
                services.AddScoped<Catalog.CatalogService>();
                services.AddScoped<OrderImportService>();
            })
            .UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(OrderImportService).Assembly);
                opts.PublishMessage<MessageEnvelope<ProductCreatedV1>>().ToLocalQueue("catalog-products-created");
                opts.LocalQueue("catalog-products-created");
            })
            .StartAsync();

        Guid productId;
        await using (var scope = host.Services.CreateAsyncScope())
        {
            var catalogService = scope.ServiceProvider.GetRequiredService<Catalog.CatalogService>();
            var result = await catalogService.CreateProductAsync(new Catalog.CreateProductRequest("Keyboard", 149.99m, 10, $"catalog-publish-{transport}"));
            productId = result.ProductId;
        }

        await Task.Delay(200);

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var orderService = scope.ServiceProvider.GetRequiredService<OrderImportService>();
            var imported = await orderService.GetProductAsync(productId);
            Assert.NotNull(imported);
        }

        await host.StopAsync();
    }
}
