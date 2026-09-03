using AgentWorkflows;
namespace AgentWorkflows.UnitTests;
public sealed class SupportWorkflowTests
{
    [Fact]
    public async Task Run_BillingTicket_FansOutToSpecialists()
    {
        var result = await new SupportWorkflow().RunAsync(new("ticket-1", "I see a duplicate charge"), TestContext.Current.CancellationToken);
        Assert.Equal("billing", result.Category);
        Assert.Equal(2, result.Findings.Count);
        Assert.Contains("payment", result.Findings.Single(x => x.Specialist == "billing").Finding);
    }

    [Fact]
    public async Task Run_TechnicalTicket_ReturnsTechnicalSummary()
    {
        var result = await new SupportWorkflow().RunAsync(new("ticket-2", "The checkout error code is 500"), TestContext.Current.CancellationToken);
        Assert.Equal("technical", result.Category);
        Assert.Contains("diagnostic", result.Summary);
    }
}