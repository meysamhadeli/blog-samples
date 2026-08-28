using System.Net;
using System.Net.Http.Json;
using Tests.Shared.TestBase;

namespace AgentTools.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AgentToolsIntegrationTests : IntegrationTestBase<Program, AgentToolsSharedFixture>
{
    public AgentToolsIntegrationTests(AgentToolsSharedFixture sharedFixture)
        : base(sharedFixture)
    {
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
    public async Task Chat_WithValidPrompt_ReturnsResponseAndSessionId()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new
            {
                sessionId = "case-1001",
                prompt = "Check order ORD-1001 and tell me its payment status."
            },
            cancellationSource.Token);
        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationSource.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("case-1001", result.SessionId);
        Assert.False(string.IsNullOrWhiteSpace(result.Response));
    }

    [Fact]
    public async Task Chat_WithoutSessionId_CreatesSessionId()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

        using var response = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new { prompt = "Explain pending charges for order ORD-1001." },
            cancellationSource.Token);
        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationSource.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.SessionId));
    }

    [Fact]
    public async Task Chat_WithDeepSeek_UsesMcpToolsQdrantAndMemory()
    {
        using var cancellationSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var sessionId = $"live-{Guid.NewGuid():N}";

        using var firstResponse = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new
            {
                sessionId,
                prompt = "My order ORD-1001 failed twice, but I see a pending charge. Is my order confirmed?"
            },
            cancellationSource.Token);
        firstResponse.EnsureSuccessStatusCode();

        var firstResult = await firstResponse.Content.ReadFromJsonAsync<ChatResponse>(cancellationSource.Token);

        Assert.NotNull(firstResult);
        Assert.Equal(sessionId, firstResult.SessionId);
        Assert.Contains("ORD-1001", firstResult.Response, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("pending", firstResult.Response, StringComparison.OrdinalIgnoreCase);

        using var secondResponse = await Client.PostAsJsonAsync(
            "/api/agent/chat",
            new
            {
                sessionId = firstResult.SessionId,
                prompt = "Should I try payment again?"
            },
            cancellationSource.Token);
        secondResponse.EnsureSuccessStatusCode();

        var secondResult = await secondResponse.Content.ReadFromJsonAsync<ChatResponse>(cancellationSource.Token);

        Assert.NotNull(secondResult);
        Assert.Equal(sessionId, secondResult.SessionId);
        Assert.Contains("retry", secondResult.Response, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ChatResponse(string SessionId, string Response);
}
