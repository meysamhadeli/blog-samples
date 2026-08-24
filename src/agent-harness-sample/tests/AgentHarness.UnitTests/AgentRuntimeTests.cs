using AgentHarness;

namespace AgentHarness.UnitTests;

public sealed class AgentRuntimeTests
{
    [Fact]
    public async Task RunAsync_ReusesSessionForSameSessionId()
    {
        var runner = new FakeAgentRunner();
        var runtime = new AgentRuntime(runner);

        await runtime.RunAsync("session-1", "first prompt", CancellationToken.None);
        await runtime.RunAsync("session-1", "second prompt", CancellationToken.None);

        Assert.Equal(1, runner.CreatedSessionCount);
        Assert.Equal(2, runner.Prompts.Count);
        Assert.Same(runner.RunSessions[0], runner.RunSessions[1]);
    }

    [Fact]
    public async Task RunAsync_CreatesDifferentSessionForDifferentSessionId()
    {
        var runner = new FakeAgentRunner();
        var runtime = new AgentRuntime(runner);

        await runtime.RunAsync("session-1", "first prompt", CancellationToken.None);
        await runtime.RunAsync("session-2", "second prompt", CancellationToken.None);

        Assert.Equal(2, runner.CreatedSessionCount);
        Assert.NotSame(runner.RunSessions[0], runner.RunSessions[1]);
    }

    private sealed class FakeAgentRunner : IAgentRunner
    {
        public int CreatedSessionCount { get; private set; }
        public List<string> Prompts { get; } = [];
        public List<object> Sessions { get; } = [];
        public List<object> RunSessions { get; } = [];

        public Task<object> CreateSessionAsync(CancellationToken cancellationToken)
        {
            var session = new object();
            CreatedSessionCount++;
            Sessions.Add(session);
            return Task.FromResult<object>(session);
        }

        public Task<string> RunAsync(
            string prompt,
            object session,
            CancellationToken cancellationToken)
        {
            Prompts.Add(prompt);
            RunSessions.Add(session);
            return Task.FromResult("test response");
        }
    }
}