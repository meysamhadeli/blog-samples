using System.Net;
using System.Net.Http.Json;
using Tests.Shared.TestBase;

namespace AgentHarness.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AgentHarnessIntegrationTests : IntegrationTestBase<Program, AgentHarnessSharedFixture>
{
    public AgentHarnessIntegrationTests(AgentHarnessSharedFixture sharedFixture) : base(sharedFixture)
    {
    }

    [Fact]
    public async Task Run_WithMissingFields_ReturnsBadRequest()
    {
        using var response = await Client.PostAsJsonAsync("/api/agent/runs", new { sessionId = "case-1001" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Run_ReturnsPendingApproval()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/agent/runs",
            new { sessionId = "case-1001", orderId = "ORD-1001", prompt = "Refund duplicate charge" });
        var result = await response.Content.ReadFromJsonAsync<RunResponse>();

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("WaitingForApproval", result.Plan.Status.ToString());
        Assert.Equal("pending", result.Approval.Status);
    }

    [Fact]
    public async Task Approval_RejectsPlanWithoutSideEffect()
    {
        var approval = await StartRunAsync();

        using var response = await Client.PostAsJsonAsync(
            $"/api/approvals/{approval.Id}", new { approved = false, reason = "Verification missing" });
        var plan = await response.Content.ReadFromJsonAsync<PlanResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(plan);
        Assert.Equal("Rejected", plan.Status.ToString());
    }

    [Fact]
    public async Task Approval_ApprovesAndCompletesPlan()
    {
        var approval = await StartRunAsync();

        using var response = await Client.PostAsJsonAsync(
            $"/api/approvals/{approval.Id}", new { approved = true, reason = "Verified duplicate charge" });
        var plan = await response.Content.ReadFromJsonAsync<PlanResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(plan);
        Assert.Equal("Completed", plan.Status.ToString());
        Assert.Contains("ORD-1001", plan.Result);
    }

    [Fact]
    public async Task Approval_WithUnknownId_ReturnsNotFound()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/approvals/missing", new { approved = true, reason = "Test" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<ApprovalResponse> StartRunAsync()
    {
        var sessionId = $"case-{Guid.NewGuid():N}";
        using var response = await Client.PostAsJsonAsync(
            "/api/agent/runs",
            new { sessionId, orderId = "ORD-1001", prompt = "Refund duplicate charge" });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RunResponse>();
        return result!.Approval;
    }

    private sealed record RunResponse(PlanResponse Plan, ApprovalResponse Approval);
    private sealed record PlanResponse(string Status, string? Result);
    private sealed record ApprovalResponse(string Id, string Status);
}