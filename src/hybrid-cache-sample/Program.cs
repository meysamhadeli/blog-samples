using HybridCacheSample.Endpoints;
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

var app = builder.Build();

// Map endpoints
app.MapUserEndpoints();

app.Run();