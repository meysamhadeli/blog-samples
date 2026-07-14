using Catalog;
using Catalog.Data;
using Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var messaging = builder.Configuration.GetSection("Messaging").Get<MessagingOptions>() ?? new MessagingOptions();
var transport = MessagingTransport.Normalize(messaging.Transport);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<CatalogWriteStore>();
builder.Services.AddSingleton<CatalogReadStore>();
builder.Services.AddSingleton(new MessagingOptions { Transport = transport });
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

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
                rider.AddProducer<MessageEnvelope<ProductCreatedV1>>("catalog-products-created");

                rider.UsingKafka((context, k) =>
                {
                    k.Host("localhost:9092");
                });
            });
            break;
    }
});
builder.Services.AddScoped<CatalogService>();

var app = builder.Build();

app.MapPost("/api/v1/catalogs/products", async (CreateProductRequestDto request, CatalogService service, CancellationToken cancellationToken) =>
{
    var result = await service.CreateProductAsync(
        new CreateProductRequest(request.Name, request.Price, request.Stock, request.CorrelationId),
        cancellationToken);

    return Results.Accepted($"/api/v1/catalogs/products/{result.ProductId}", result);
});

app.MapGet("/api/v1/catalogs/products/{id:guid}", async (Guid id, CatalogService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetProductAsync(id, cancellationToken);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.MapGet("/api/v1/catalogs/products/read-model/{id:guid}", async (Guid id, CatalogService service, CancellationToken cancellationToken) =>
{
    var product = await service.GetReadModelAsync(id, cancellationToken);
    return product is null ? Results.NotFound() : Results.Ok(product);
});

app.Run();

public sealed record CreateProductRequestDto(string Name, decimal Price, int Stock, string? CorrelationId);

public partial class Program;
