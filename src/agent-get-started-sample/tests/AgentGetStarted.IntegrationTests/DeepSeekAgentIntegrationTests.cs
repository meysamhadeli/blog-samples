using System.Net.Http.Json;
using System.Net;
using AgentGetStarted;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        using var factory = CreateFactory(out _);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/agent/chat",
            new { prompt = " " });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Chat_WithValidPrompt_ReturnsAgentResponse()
    {
        using var factory = CreateFactory(out var runner);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/agent/chat",
            new { prompt = "Explain checkout failure." });
        var result = await response.Content.ReadFromJsonAsync<ChatResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("fake agent response", result.Response);
        Assert.Equal("Explain checkout failure.", runner.LastPrompt);
    }

    [Fact]
    public async Task Chat_WithMissingPrompt_ReturnsBadRequest()
    {
        using var factory = CreateFactory(out _);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/agent/chat",
            new { });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static FakeAgentWebApplicationFactory CreateFactory(out FakeAgentRunner runner)
    {
        runner = new FakeAgentRunner();
        return new FakeAgentWebApplicationFactory(runner);
    }

    private sealed record ChatResponse(string Response);

    private sealed class FakeAgentRunner : IAgentRunner
    {
        public string? LastPrompt { get; private set; }

        public Task<string> RunAsync(string prompt, CancellationToken cancellationToken)
        {
            LastPrompt = prompt;
            return Task.FromResult("fake agent response");
        }
    }

    private sealed class FakeAgentWebApplicationFactory(FakeAgentRunner runner)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IAgentRunner>();
                services.AddSingleton<IAgentRunner>(runner);
            });
        }
    }
}
