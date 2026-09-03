using System.Text.Json.Serialization;
using AgentHarnessSample;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AgentHarness>();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var apiKey = Environment.GetEnvironmentVariable("DS_KEY");
if (!string.IsNullOrWhiteSpace(apiKey))
{
    builder.Services.AddSingleton(new SupportAgent(
        apiKey,
        builder.Configuration["DeepSeek:Endpoint"] ?? "https://api.deepseek.com",
        builder.Configuration["DeepSeek:Model"] ?? "deepseek-v4-flash"));
}

var app = builder.Build();

app.MapPost("api/agent/runs", (RunRequest request, AgentHarness harness) =>
{
    if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.OrderId))
        return Results.BadRequest("SessionId and OrderId are required.");
    var plan = harness.Start(request.SessionId.Trim(), request.OrderId.Trim());
    var pending = harness.GetPending(plan.SessionId);
    return Results.Accepted($"api/agent/runs/{plan.SessionId}", new { plan, approval = pending.Approval });
});

app.MapPost("api/approvals/{approvalId}", (string approvalId, ApprovalDecision decision, AgentHarness harness) =>
{
    if (string.IsNullOrWhiteSpace(decision.Reason))
        return Results.BadRequest("Reason is required.");
    try
    {
        return Results.Ok(harness.Decide(approvalId, decision));
    }
    catch (KeyNotFoundException exception) { return Results.NotFound(exception.Message); }
    catch (InvalidOperationException exception) { return Results.Conflict(exception.Message); }
});

app.MapGet("api/agent/runs/{sessionId}", (string sessionId, AgentHarness harness) =>
{
    try { return Results.Ok(harness.GetPlan(sessionId)); }
    catch (KeyNotFoundException exception) { return Results.NotFound(exception.Message); }
});

app.MapPost("api/agent/explain", async (ExplainRequest request, SupportAgent? agent, CancellationToken cancellationToken) =>
{
    if (agent is null) return Results.Problem("Set DS_KEY to enable model explanations.", statusCode: 503);
    return Results.Ok(new { response = await agent.ExplainAsync(request.Prompt, cancellationToken) });
});

app.Run();

public record RunRequest(string SessionId, string OrderId, string Prompt);
public record ExplainRequest(string Prompt);
public partial class Program;