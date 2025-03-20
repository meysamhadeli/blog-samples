using Microsoft.Extensions.Caching.Hybrid;

namespace HybridCacheSample.Services;

public class ProductService(HybridCache cache)
{
    // Example of using tags
    public async Task<string> GetProductInfoAsync(string productId, CancellationToken token = default)
    {
        var tags = new List<string> { "category_1" };
        return await cache.GetOrCreateAsync(
            $"product_{productId}", // Cache key
            async cancel =>
            {
                // Simulate a slow database call
                Console.WriteLine($"Fetching product info for {productId} from the database...");
                await Task.Delay(2000, cancel); // Simulate a 2-second delay
                return $"Product info for {productId}";
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) },
            tags: tags,
            cancellationToken: token
        );
    }
}