using LangfuseGenAI.Services;

namespace LangfuseGenAI.Endpoints;

internal static class ChatEndpoints
{
    internal static RouteGroupBuilder MapChatEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/chat").WithTags("Chat");

        group.MapPost("/", HandleChatAsync)
            .WithName("ChatCompletion")
            .Produces<ChatResponse>();

        return group;
    }

    private static async Task<IResult> HandleChatAsync(
        ChatRequest request,
        ChatService chatService,
        CancellationToken cancellationToken)
    {
        var result = await chatService.ProcessChatAsync(
            request.Message,
            request.SessionId,
            request.UserId,
            cancellationToken);

        return Results.Ok(new ChatResponse(result));
    }
}

internal sealed record ChatRequest(string Message, string? SessionId, string? UserId);
internal sealed record ChatResponse(string Reply);
