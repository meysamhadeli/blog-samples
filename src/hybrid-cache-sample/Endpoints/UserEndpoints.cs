using HybridCacheSample.Services;

namespace HybridCacheSample.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoint to fetch user info
        app.MapGet("/user/{userId}", async (string userId, UserService userService) =>
        {
            var userInfo = await userService.GetUserInfoAsync(userId);
            return Results.Ok(userInfo);
        });

        // Endpoint to demonstrate stampede protection
        app.MapGet("/test-stampede", async (UserService userService) =>
        {
            var userId = "123";
            var tasks = new List<Task<string>>();

            // Simulate 10 concurrent requests
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(userService.GetUserInfoAsync(userId));
            }

            // Wait for all tasks to complete
            var results = await Task.WhenAll(tasks);

            return Results.Ok(results);
        });
    }
}