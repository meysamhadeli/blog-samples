using Contracts;
using Order;
using Order.Data;
using Wolverine;
using Wolverine.Kafka;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var messaging = builder.Configuration.GetSection("Messaging").Get<MessagingOptions>() ?? new MessagingOptions();
var transport = MessagingTransport.Normalize(messaging.Transport);

builder.Host.UseWolverine(opts =>
{
    opts.Discovery.IncludeAssembly(typeof(OrderImportService).Assembly);

    switch (transport)
    {
        case MessagingTransport.RabbitMq:
            opts.UseRabbitMq(new Uri("amqp://guest:guest@localhost:5672"));
            opts.ListenToRabbitQueue("catalog-products-created").UseDurableInbox();
            break;

        case MessagingTransport.Kafka:
            opts.UseKafka("localhost:9092").AutoProvision();
            opts.ListenToKafkaTopic("catalog-products-created").UseDurableInbox();
            break;
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<OrderImportStore>();
builder.Services.AddSingleton<InboxStore>();
builder.Services.AddScoped<OrderImportService>();
builder.Services.AddSingleton(new MessagingOptions { Transport = transport });

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
