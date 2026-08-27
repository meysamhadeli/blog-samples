using AgentGetStarted;

namespace AgentGetStarted.UnitTests;

public sealed class AgentRuntimeTests
{
    [Fact]
    public async Task RunAsync_ForwardsPromptAndReturnsAgentResponse()
    {
        var runner = new FakeAgentRunner("test response");
        var runtime = new AgentRuntime(runner);

        var response = await runtime.RunAsync("first prompt", CancellationToken.None);

        Assert.Equal("test response", response);
        Assert.Equal(["first prompt"], runner.Prompts);
    }

    [Fact]
    public async Task RunAsync_ForwardsCancellationToken()
    {
        var runner = new FakeAgentRunner("test response");
        var runtime = new AgentRuntime(runner);
        using var cancellationSource = new CancellationTokenSource();

        await runtime.RunAsync("prompt", cancellationSource.Token);

        Assert.Equal(cancellationSource.Token, runner.CancellationToken);
    }

    [Fact]
    public async Task RunAsync_ForwardsEachPromptToRunner()
    {
        var runner = new FakeAgentRunner("test response");
        var runtime = new AgentRuntime(runner);

        await runtime.RunAsync("first prompt", CancellationToken.None);
        await runtime.RunAsync("second prompt", CancellationToken.None);

        Assert.Equal(["first prompt", "second prompt"], runner.Prompts);
    }

    [Fact]
    public async Task RunAsync_PropagatesRunnerException()
    {
        var exception = new InvalidOperationException("provider failed");
        var runtime = new AgentRuntime(new ThrowingAgentRunner(exception));

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => runtime.RunAsync("prompt", CancellationToken.None));

        Assert.Same(exception, actual);
    }

    private sealed class FakeAgentRunner(string response) : IAgentRunner
    {
        public List<string> Prompts { get; } = [];
        public CancellationToken CancellationToken { get; private set; }

        public Task<string> RunAsync(string prompt, CancellationToken cancellationToken)
        {
            Prompts.Add(prompt);
            CancellationToken = cancellationToken;
            return Task.FromResult(response);
        }
    }

    private sealed class ThrowingAgentRunner(Exception exception) : IAgentRunner
    {
        public Task<string> RunAsync(string prompt, CancellationToken cancellationToken) =>
            Task.FromException<string>(exception);
    }
}
