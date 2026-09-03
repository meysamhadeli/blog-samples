using System.Net;
using System.Net.Http.Json;
using Tests.Shared.Fixtures;
using Tests.Shared.TestBase;
namespace AgentWorkflows.IntegrationTests;
public sealed class WorkflowFixture : SharedFixture<Program>;

[CollectionDefinition(Name)]
public sealed class WorkflowTestCollection : ICollectionFixture<WorkflowFixture>
{
    public const string Name = "Integration tests";
}

[Collection(WorkflowTestCollection.Name)]
public sealed class WorkflowIntegrationTests : IntegrationTestBase<Program, WorkflowFixture>
{
    public WorkflowIntegrationTests(WorkflowFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Workflow_WithEmptyText_ReturnsBadRequest()
    {
        using var response = await Client.PostAsJsonAsync("/api/workflows/support", new { ticketId = "ticket-1", text = " " }, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Workflow_WithTicket_ReturnsFindings()
    {
        using var response = await Client.PostAsJsonAsync("/api/workflows/support", new { ticketId = "ticket-1", text = "duplicate charge" }, TestContext.Current.CancellationToken);
        var result = await response.Content.ReadFromJsonAsync<WorkflowResponse>(TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("billing", result.Category);
        Assert.Equal(2, result.Findings.Count);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var response = await Client.GetAsync("/api/health", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record WorkflowResponse(string TicketId, string Category, IReadOnlyList<Finding> Findings, string Summary);
    private sealed record Finding(string Specialist, string FindingText);
}