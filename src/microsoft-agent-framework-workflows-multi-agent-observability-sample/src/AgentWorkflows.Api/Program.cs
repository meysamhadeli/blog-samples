using AgentWorkflows;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<SupportWorkflow>();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
        .AddSource("AgentWorkflows.Workflow").AddOtlpExporter())
    .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation()
        .AddMeter("AgentWorkflows.Workflow").AddOtlpExporter().AddPrometheusExporter())
    .WithLogging(logging => logging.AddOtlpExporter());

var app = builder.Build();
app.MapPost("api/workflows/support", async (TicketRequest request, SupportWorkflow workflow, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.TicketId) || string.IsNullOrWhiteSpace(request.Text))
        return Results.BadRequest("TicketId and Text are required.");
    logger.LogInformation("Support workflow started for ticket {TicketId}", request.TicketId);
    var result = await workflow.RunAsync(request, cancellationToken);
    logger.LogInformation("Support workflow completed for ticket {TicketId} with category {Category}", result.TicketId, result.Category);
    return Results.Ok(result);
});
app.MapGet("api/health", () => Results.Ok(new { status = "ok" }));
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.Run();
public partial class Program;