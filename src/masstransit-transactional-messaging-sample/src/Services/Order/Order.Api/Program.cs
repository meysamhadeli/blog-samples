using Contracts;
using MassTransit;
using Order;
using Order.Data;

var builder = WebApplication.CreateBuilder(args);
var messaging = builder.Configuration.GetSection("Messaging").Get<MessagingOptions>() ?? new MessagingOptions();
var transport = MessagingTransport.Normalize(messaging.Transport);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<OrderImportStore>();
builder.Services.AddSingleton<InboxStore>();
builder.Services.AddSingleton(new MessagingOptions { Transport = transport });
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<OrderImportService>();

    switch (transport)
    {
        case MessagingTransport.RabbitMq:
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ConfigureEndpoints(context);
            });
            break;
        case MessagingTransport.Kafka:
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(rider =>
            {
                rider.AddConsumer<OrderImportService>();

                rider.UsingKafka((context, k) =>
                {
                    k.Host("localhost:9092");
                    k.TopicEndpoint<MessageEnvelope<ProductCreatedV1>>(
                        "catalog-products-created",
                        "order-import-service",
                        e =>
                        {
                            e.ConfigureConsumer<OrderImportService>(context);
                        });
                });
            });
            break;
    }
});
builder.Services.AddSingleton<OrderImportService>();

var app = builder.Build();

app.MapPost("/api/v1/orders/products/import", async (MessageEnvelope<ProductCreatedV1> message, OrderImportService service, CancellationToken cancellationToken) =>
{
    var imported = await service.ImportAsync(message, cancellationToken);
    return imported ? Results.Accepted($"/api/v1/orders/products/{message.Data.ProductId}") : Results.Ok(new { Duplicate = true });
});

app.MapGet("/api/v1/orders/products/{id:guid}", async (Guid id, OrderImportService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetProductAsync(id, cancellationToken);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.Run();

public partial class Program;
