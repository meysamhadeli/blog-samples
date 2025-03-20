using HybridCacheSample.Services;

namespace HybridCacheSample.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint to demonstrate stampede protection
        app.MapGet("/product/test-stampede", async (ProductService productService) =>
        {
            var productId = "123";
            var tasks = new List<Task<string>>();

            // Simulate 10 concurrent requests
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(productService.GetProductInfoAsync(productId));
            }

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            return Results.Ok(results);
        })
        .WithName("TestStampedeProtection")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Test stampede protection",
            Description = "Simulates multiple concurrent requests to demonstrate HybridCache's stampede protection."
        });
    }
}