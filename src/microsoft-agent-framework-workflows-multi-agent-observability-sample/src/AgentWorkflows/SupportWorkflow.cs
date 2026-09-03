using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentWorkflows;

public sealed record TicketRequest(string TicketId, string Text);
public sealed record SpecialistFinding(string Specialist, string Finding);
public sealed record WorkflowResult(string TicketId, string Category, IReadOnlyList<SpecialistFinding> Findings, string Summary);

public static class WorkflowTelemetry
{
    public static readonly ActivitySource Source = new("AgentWorkflows.Workflow");
    public static readonly Meter Meter = new("AgentWorkflows.Workflow");
    public static readonly Counter<long> Runs = Meter.CreateCounter<long>("agent.workflow.runs");
    public static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("agent.workflow.duration", "ms");
}

public interface IWorkflowExecutor
{
    Task<SpecialistFinding> ExecuteAsync(TicketRequest request, CancellationToken cancellationToken);
}

public sealed class BillingSpecialist : IWorkflowExecutor
{
    public Task<SpecialistFinding> ExecuteAsync(TicketRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new SpecialistFinding("billing", request.Text.Contains("charge", StringComparison.OrdinalIgnoreCase)
            ? "Inspect the payment authorization and avoid repeated payment attempts."
            : "No billing signal found."));
}

public sealed class TechnicalSpecialist : IWorkflowExecutor
{
    public Task<SpecialistFinding> ExecuteAsync(TicketRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new SpecialistFinding("technical", request.Text.Contains("error", StringComparison.OrdinalIgnoreCase)
            ? "Collect the error code and request correlation ID before escalation."
            : "No technical signal found."));
}

public sealed class SupportWorkflow
{
    private readonly IReadOnlyList<IWorkflowExecutor> specialists = [new BillingSpecialist(), new TechnicalSpecialist()];

    public async Task<WorkflowResult> RunAsync(TicketRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = WorkflowTelemetry.Source.StartActivity("workflow.session", ActivityKind.Internal);
        activity?.SetTag("workflow.id", "support-ticket");
        activity?.SetTag("session.id", request.TicketId);
        var stopwatch = Stopwatch.StartNew();
        WorkflowTelemetry.Runs.Add(1);
        var category = request.Text.Contains("charge", StringComparison.OrdinalIgnoreCase) ? "billing" : "technical";
        var findings = await Task.WhenAll(specialists.Select(specialist => ExecuteAsync(specialist, request, cancellationToken)));
        var summary = category == "billing"
            ? "Billing specialist recommends payment review before any retry."
            : "Technical specialist recommends collecting diagnostic details before escalation.";
        activity?.SetTag("workflow.category", category);
        WorkflowTelemetry.Duration.Record(stopwatch.Elapsed.TotalMilliseconds);
        return new WorkflowResult(request.TicketId, category, findings, summary);
    }

    private static async Task<SpecialistFinding> ExecuteAsync(IWorkflowExecutor executor, TicketRequest request, CancellationToken cancellationToken)
    {
        using var activity = WorkflowTelemetry.Source.StartActivity("executor.process", ActivityKind.Internal);
        activity?.SetTag("executor.id", executor.GetType().Name);
        return await executor.ExecuteAsync(request, cancellationToken);
    }
}

public sealed class SupportAgent
{
    private readonly AIAgent agent;

    public SupportAgent(string apiKey, string endpoint, string model)
    {
        IChatClient client = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
            .GetChatClient(model).AsIChatClient();
        agent = client.AsAIAgent(instructions: "Summarize workflow findings. Do not invent ticket or tool results.");
    }

    public async Task<string> SummarizeAsync(WorkflowResult result, CancellationToken cancellationToken) =>
        (await agent.RunAsync($"Ticket {result.TicketId}: {result.Summary}", cancellationToken: cancellationToken)).Text ?? string.Empty;
}