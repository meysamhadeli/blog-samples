using System.Text.Json;
using System.Text.Json.Serialization;
using Catalog;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddOpenApi();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapGet("/", () => Results.Ok(new { service = nameof(CatalogModule), status = "running" }));
app.MapApplicationEndpoints();
app.MapDefaultEndpoints();
app.Run();
public partial class Program;
