using System.Net.Http.Json;
using System.Net;
using AgentGetStarted;
using Tests.Shared.TestBase;

namespace AgentGetStarted.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DeepSeekAgentIntegrationTests : IntegrationTestBase<Program, AgentGetStartedSharedFixture>
{
    public DeepSeekAgentIntegrationTests(AgentGetStartedSharedFixture sharedFixture)
        : base(sharedFixture)
    {
    }

    [Fact]
    public async Task Chat_WithDeepSeek_ReturnsResponse()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new { prompt = "Reply with exactly: get started agent integration ok" },
            cancellationSource.Token);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationSource.Token);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Response));
    }

    [Fact]
    public async Task Chat_WithEmptyPrompt_ReturnsBadRequest()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new { prompt = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Chat_WithDeepSeek_ReturnsNonEmptyResponse()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new { prompt = "Explain checkout failure in one sentence." },
            cancellationSource.Token);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationSource.Token);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Response));
    }

    [Fact]
    public async Task Chat_WithMissingPrompt_ReturnsBadRequest()
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record ChatResponse(string Response);
}
