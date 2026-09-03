using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentHarnessSample;

public enum PlanStatus
{
    WaitingForApproval,
    Running,
    Completed,
    Rejected
}

public sealed class SupportPlan
{
    public required string SessionId { get; init; }
    public required string OrderId { get; init; }
    public PlanStatus Status { get; set; }
    public string? ApprovalId { get; set; }
    public string? Result { get; set; }
}

public sealed record ApprovalRequest(
    string Id,
    string SessionId,
    string ToolName,
    string Arguments,
    string Status,
    string? DecisionReason = null);

public sealed record ApprovalDecision(bool Approved, string Reason);

public sealed class AgentHarness
{
    private readonly Dictionary<string, SupportPlan> plans = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ApprovalRequest> approvals = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> refunds = new(StringComparer.Ordinal);
    private readonly object sync = new();

    public SupportPlan Start(string sessionId, string orderId)
    {
        lock (sync)
        {
            var plan = new SupportPlan
            {
                SessionId = sessionId,
                OrderId = orderId,
                Status = PlanStatus.WaitingForApproval,
                ApprovalId = Guid.NewGuid().ToString("N")
            };
            plans[sessionId] = plan;
            approvals[plan.ApprovalId] = new(
                plan.ApprovalId, sessionId, "refund_order", $"{{\"orderId\":\"{orderId}\"}}", "pending");
            return plan;
        }
    }

    public (SupportPlan Plan, ApprovalRequest Approval) GetPending(string sessionId)
    {
        lock (sync)
        {
            var plan = GetPlan(sessionId);
            if (plan.ApprovalId is null || !approvals.TryGetValue(plan.ApprovalId, out var approval))
                throw new InvalidOperationException("Plan has no pending approval.");
            return (plan, approval);
        }
    }

    public SupportPlan Decide(string approvalId, ApprovalDecision decision)
    {
        lock (sync)
        {
            if (!approvals.TryGetValue(approvalId, out var approval))
                throw new KeyNotFoundException("Approval request was not found.");
            if (approval.Status != "pending")
                throw new InvalidOperationException("Approval request was already decided.");

            var plan = GetPlan(approval.SessionId);
            var status = decision.Approved ? "approved" : "rejected";
            approvals[approvalId] = approval with { Status = status, DecisionReason = decision.Reason };
            if (!decision.Approved)
            {
                plan.Status = PlanStatus.Rejected;
                plan.Result = "Refund was not executed because approval was rejected.";
                return plan;
            }

            plan.Status = PlanStatus.Running;
            plan.Result = RefundOrder(plan.OrderId);
            plan.Status = PlanStatus.Completed;
            return plan;
        }
    }

    public SupportPlan GetPlan(string sessionId)
    {
        if (!plans.TryGetValue(sessionId, out var plan))
            throw new KeyNotFoundException("Plan was not found.");
        return plan;
    }

    private string RefundOrder(string orderId)
    {
        if (refunds.TryGetValue(orderId, out var existing))
            return existing;
        var result = $"Refund queued for {orderId}.";
        refunds[orderId] = result;
        return result;
    }
}

public sealed class SupportAgent
{
    private readonly AIAgent agent;

    public SupportAgent(string apiKey, string endpoint, string model)
    {
        IChatClient chatClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
            .GetChatClient(model)
            .AsIChatClient();
        agent = chatClient.AsAIAgent(
            instructions: "Explain support plans clearly. Never claim a refund happened unless the harness result says it did.");
    }

    public async Task<string> ExplainAsync(string prompt, CancellationToken cancellationToken) =>
        (await agent.RunAsync(prompt, cancellationToken: cancellationToken)).Text ?? string.Empty;
}