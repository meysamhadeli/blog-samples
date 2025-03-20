
using Microsoft.Extensions.Caching.Hybrid;

namespace HybridCacheSample.Services;

public class UserService(HybridCache cache)
{
    public async Task<string> GetUserInfoAsync(string userId, CancellationToken token = default)
    {
        return await cache.GetOrCreateAsync(
            $"user_{userId}", // Cache key
            async cancel =>
            {
                // Simulate a slow database call
                Console.WriteLine($"Fetching user info for {userId} from the database...");
                await Task.Delay(2000, cancel); // Simulate a 2-second delay
                return $"User info for {userId}";
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(5) }, // Cache for 5 minutes
            cancellationToken: token
        );
    }

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

    // Example of removing a cache entry
    public async Task RemoveUserInfoAsync(string userId)
    {
        await cache.RemoveAsync($"user_{userId}");
    }

    // Example of setting a cache entry manually
    public async Task SetUserInfoAsync(string userId, string userInfo)
    {
        await cache.SetAsync($"user_{userId}", userInfo);
    }
}