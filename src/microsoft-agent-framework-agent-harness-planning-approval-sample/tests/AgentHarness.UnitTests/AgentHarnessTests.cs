using AgentHarnessSample;

namespace AgentHarness.UnitTests;

public sealed class AgentHarnessTests
{
    [Fact]
    public void Start_CreatesPlanWaitingForApproval()
    {
        var harness = new AgentHarnessSample.AgentHarness();

        var plan = harness.Start("case-1001", "ORD-1001");

        Assert.Equal(PlanStatus.WaitingForApproval, plan.Status);
        Assert.NotNull(plan.ApprovalId);
        Assert.Equal("refund_order", harness.GetPending("case-1001").Approval.ToolName);
    }

    [Fact]
    public void DecideRejectsWithoutExecutingRefund()
    {
        var harness = new AgentHarnessSample.AgentHarness();
        var plan = harness.Start("case-1001", "ORD-1001");

        var result = harness.Decide(plan.ApprovalId!, new(false, "Verification missing"));

        Assert.Equal(PlanStatus.Rejected, result.Status);
        Assert.Contains("not executed", result.Result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DecideApproveCompletesIdempotentRefund()
    {
        var harness = new AgentHarnessSample.AgentHarness();
        var plan = harness.Start("case-1001", "ORD-1001");

        var result = harness.Decide(plan.ApprovalId!, new(true, "Verified duplicate charge"));

        Assert.Equal(PlanStatus.Completed, result.Status);
        Assert.Equal("Refund queued for ORD-1001.", result.Result);
        Assert.Throws<InvalidOperationException>((Action)(() =>
            harness.Decide(plan.ApprovalId!, new(true, "Duplicate callback"))));
    }
}