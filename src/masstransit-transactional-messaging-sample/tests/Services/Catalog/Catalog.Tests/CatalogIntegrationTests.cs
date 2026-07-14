using Xunit;
using Catalog.Models;
using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Order;
using Order.Data;
using Tests.Shared;

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
    public async Task create_product_should_publish_integration_event_that_order_consumer_can_handle(string transport)
    {
        await using var provider = new ServiceCollection()
            .AddSingleton<Catalog.Data.CatalogWriteStore>()
            .AddSingleton<Catalog.Data.CatalogReadStore>()
            .AddSingleton<OrderImportStore>()
            .AddSingleton<InboxStore>()
            .AddMassTransit(x =>
            {
                x.AddConsumer<OrderImportService>();
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            })
            .AddScoped<Catalog.CatalogService>()
            .BuildServiceProvider(true);

        var busControl = provider.GetRequiredService<IBusControl>();
        await busControl.StartAsync();

        try
        {
            await using var scope = provider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<Catalog.CatalogService>();
            var result = await service.CreateProductAsync(
                new Catalog.CreateProductRequest("Mechanical Keyboard", 149.99m, 10, $"catalog-mt-{transport}"));

            await Task.Delay(200);

            var orderScope = provider.CreateScope();
            var orderService = orderScope.ServiceProvider.GetRequiredService<OrderImportService>();
            var imported = await orderService.GetProductAsync(result.ProductId);
            Assert.NotNull(imported);
        }
        finally
        {
            await busControl.StopAsync();
            await provider.DisposeAsync();
        }
    }
}
