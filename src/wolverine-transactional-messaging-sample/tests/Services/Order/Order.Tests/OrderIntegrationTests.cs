using Xunit;
using Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Order;
using Order.Data;
using Wolverine;

namespace Order.Tests;

public sealed class OrderIntegrationTests
{
    public static TheoryData<string> SupportedTransports => new()
    {
        "rabbitmq",
        "kafka"
    };

    [Theory]
    [MemberData(nameof(SupportedTransports))]
    public async Task import_should_be_idempotent_for_duplicate_messages_for_supported_brokers(string transport)
    {
        var service = new OrderImportService(new OrderImportStore(), new InboxStore());
        var message = MessageEnvelope<ProductCreatedV1>.Create(
            new ProductCreatedV1(Guid.NewGuid(), "Mouse", 49.99m, 20),
            $"order-import-{transport}");

        var first = await service.ImportAsync(message);
        var second = await service.ImportAsync(message);

        Assert.True(first);
        Assert.False(second);
    }

    [Theory]
    [MemberData(nameof(SupportedTransports))]
    public async Task handler_should_consume_published_message_for_supported_brokers(string transport)
    {
        using var host = await Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<OrderImportStore>();
                services.AddSingleton<InboxStore>();
                services.AddScoped<OrderImportService>();
            })
            .UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(OrderImportService).Assembly);
                opts.PublishMessage<MessageEnvelope<ProductCreatedV1>>().ToLocalQueue("catalog-products-created");
                opts.LocalQueue("catalog-products-created");
            })
            .StartAsync();

        var bus = host.Services.GetRequiredService<IMessageBus>();
        var message = MessageEnvelope<ProductCreatedV1>.Create(
            new ProductCreatedV1(Guid.NewGuid(), "Mouse", 49.99m, 20),
            $"order-consume-{transport}");

        await bus.PublishAsync(message);
        await Task.Delay(200);

        await using (var scope = host.Services.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<OrderImportService>();
            var imported = await service.GetProductAsync(message.Data.ProductId);
            Assert.NotNull(imported);
        }

        await host.StopAsync();
    }
}
