using System.Text.Json;
using System.Text.Json.Serialization;
using backend;
using Microsoft.AspNetCore.Http.Json;
using backend.Shared.Extensions.HostApplicationBuilderExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();
builder.AddBackendInfrastructure();
builder.Services.AddOpenApi();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

var app = builder.Build();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapGet("/", () => Results.Ok(new { service = nameof(BackendModule), status = "running" }));
app.UseBackendInfrastructure();
app.MapApplicationEndpoints();
app.Run();

public partial class Program;
