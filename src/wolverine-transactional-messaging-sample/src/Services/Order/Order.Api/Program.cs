using Contracts;
using Order;
using Order.Data;

var builder = WebApplication.CreateBuilder(args);
var messaging = builder.Configuration.GetSection("Messaging").Get<MessagingOptions>() ?? new MessagingOptions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<OrderImportStore>();
builder.Services.AddSingleton<InboxStore>();
builder.Services.AddSingleton<OrderImportService>();
builder.Services.AddSingleton(messaging);

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
