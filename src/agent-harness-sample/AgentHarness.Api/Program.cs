using AgentHarness;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AgentRuntime>(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var apiKey = Environment.GetEnvironmentVariable("DS_KEY")
        ?? throw new InvalidOperationException("Configure the DS_KEY environment variable.");
    var endpoint = configuration["DeepSeek:Endpoint"] ?? "https://api.deepseek.com";
    var model = configuration["DeepSeek:Model"] ?? "deepseek-v4-flash";

    return new AgentRuntime(apiKey, endpoint, model);
});

var app = builder.Build();

app.MapPost("api/agent/chat", async (
    ChatRequest request,
    IServiceProvider services,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
        return Results.BadRequest("Prompt is required.");

    var runtime = services.GetRequiredService<AgentRuntime>();
    var sessionId = string.IsNullOrWhiteSpace(request.SessionId)
        ? Guid.NewGuid().ToString("N")
        : request.SessionId;
    var response = await runtime.RunAsync(sessionId, request.Prompt, cancellationToken);

    return Results.Ok(new ChatResponse(sessionId, response));
});

app.Run();

public record ChatRequest(string? SessionId, string Prompt);

public record ChatResponse(string SessionId, string Response);

public partial class Program;