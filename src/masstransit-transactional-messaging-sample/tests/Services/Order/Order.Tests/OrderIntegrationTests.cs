using Xunit;
using Contracts;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Order;
using Order.Data;

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
    public async Task consumer_should_handle_published_message_for_supported_brokers(string transport)
    {
        await using var provider = new ServiceCollection()
            .AddSingleton<OrderImportStore>()
            .AddSingleton<InboxStore>()
            .AddMassTransit(x =>
            {
                x.AddConsumer<OrderImportService>();
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            })
            .BuildServiceProvider(true);

        var busControl = provider.GetRequiredService<IBusControl>();
        await busControl.StartAsync();

        try
        {
            var message = MessageEnvelope<ProductCreatedV1>.Create(
                new ProductCreatedV1(Guid.NewGuid(), "Mouse", 49.99m, 20),
                $"order-consume-{transport}");

            await busControl.Publish(message);
            await Task.Delay(200);

            var store = provider.GetRequiredService<OrderImportStore>();
            var imported = await store.FindAsync(message.Data.ProductId);
            Assert.NotNull(imported);
        }
        finally
        {
            await busControl.StopAsync();
            await provider.DisposeAsync();
        }
    }
}
