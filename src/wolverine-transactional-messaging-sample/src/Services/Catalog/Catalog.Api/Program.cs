using Catalog;
using Catalog.Data;
using Contracts;

var builder = WebApplication.CreateBuilder(args);
var messaging = builder.Configuration.GetSection("Messaging").Get<MessagingOptions>() ?? new MessagingOptions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<CatalogWriteStore>();
builder.Services.AddSingleton<CatalogReadStore>();
builder.Services.AddSingleton<CatalogService>();
builder.Services.AddSingleton(messaging);

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
