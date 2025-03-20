using HybridCacheSample;
using Microsoft.Extensions.Caching.Hybrid;
using HybridCacheSample.Services;

var builder = WebApplication.CreateBuilder(args);

// Register HybridCache with custom options
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(5), // Default expiration for both caches
        LocalCacheExpiration = TimeSpan.FromMinutes(2) // Expiration for in-memory cache only
    };
});

// Register Redis as the distributed cache (optional)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Register the UserService
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new() { Title = "HybridCache Demo API", Version = "v1" }); });

var app = builder.Build();

// Enable Swagger UI in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "HybridCache Demo API v1"); });
}

// Map endpoints
app.MapEndpoints();

app.Run();