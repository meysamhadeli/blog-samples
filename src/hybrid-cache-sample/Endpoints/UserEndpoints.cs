using HybridCacheSample.Dtos;
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
            })
            .WithName("GetUserInfo")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Get user information",
                Description = "Fetches user information from the cache or database."
            });

        // Endpoint to remove user info from the cache
        app.MapDelete("/user/{userId}", async (string userId, UserService userService) =>
            {
                await userService.RemoveUserInfoAsync(userId);
                return Results.Ok($"User info for {userId} has been removed from the cache.");
            })
            .WithName("RemoveUserInfo")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Remove user information",
                Description = "Removes user information from the cache."
            });


        // Endpoint to set user info in the cache
        app.MapPost("/user", async (SetUserInfoRequestDto request, UserService userService) =>
            {
                await userService.SetUserInfoAsync(request.UserId, request.UserInfo);
                return Results.Ok($"User info for {request.UserId} has been set in the cache.");
            })
            .WithName("SetUserInfo")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Set user information",
                Description = "Manually sets user information in the cache."
            });
    }
}