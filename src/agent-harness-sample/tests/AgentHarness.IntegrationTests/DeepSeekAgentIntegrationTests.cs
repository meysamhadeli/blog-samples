using System.Net.Http.Json;
using Tests.Shared.TestBase;

namespace AgentHarness.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DeepSeekAgentIntegrationTests : DeepSeekIntegrationTestBase
{
    public DeepSeekAgentIntegrationTests(AgentHarnessSharedFixture sharedFixture)
        : base(sharedFixture)
    {
    }

    [Fact]
    public async Task Chat_UsesDeepSeekHarnessAndReturnsResponse()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var result = await SendChatAsync(
            new { prompt = "Reply with exactly: customer support harness integration ok" },
            cancellationSource.Token);

        Assert.False(string.IsNullOrWhiteSpace(result.SessionId));
        Assert.False(string.IsNullOrWhiteSpace(result.Response));
        Assert.Contains("customer support harness integration ok", result.Response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Chat_PreservesProvidedSessionIdAcrossRequests()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var sessionId = $"integration-{Guid.NewGuid():N}";

        var firstResponse = await SendChatAsync(
            new
            {
                sessionId,
                prompt = "Reply with exactly: first support turn"
            },
            cancellationSource.Token);
        var secondResponse = await SendChatAsync(
            new
            {
                sessionId,
                prompt = "Reply with exactly: second support turn"
            },
            cancellationSource.Token);

        Assert.Equal(sessionId, firstResponse.SessionId);
        Assert.Equal(sessionId, secondResponse.SessionId);
        Assert.Contains("second support turn", secondResponse.Response, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Chat_RemembersConversationContextAcrossRequests()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var sessionId = $"context-{Guid.NewGuid():N}";

        await SendChatAsync(
            new
            {
                sessionId,
                prompt = "Remember this support ticket code: ticket-741"
            },
            cancellationSource.Token);
        var secondResponse = await SendChatAsync(
            new
            {
                sessionId,
                prompt = "What support ticket code did I ask you to remember? Reply with only the code."
            },
            cancellationSource.Token);

        Assert.Contains("ticket-741", secondResponse.Response, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<ChatResponse> SendChatAsync(object request, CancellationToken cancellationToken)
    {
        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken);

        Assert.NotNull(result);
        return result;
    }

    private sealed record ChatResponse(string SessionId, string Response);
}