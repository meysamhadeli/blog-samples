using System.Collections.Concurrent;
using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentHarness;

public interface IAgentRunner
{
    Task<object> CreateSessionAsync(CancellationToken cancellationToken);
    Task<string> RunAsync(string prompt, object session, CancellationToken cancellationToken);
}

public sealed class AgentRuntime
{
    private readonly ConcurrentDictionary<string, object> _sessions = new();
    private readonly IAgentRunner _runner;

    public AgentRuntime(string apiKey, string endpoint, string model)
        : this(new HarnessAgentRunner(CreateAgent(apiKey, endpoint, model)))
    {
    }

    public AgentRuntime(IAgentRunner runner)
    {
        _runner = runner;
    }

    public async Task<string> RunAsync(string sessionId, string prompt, CancellationToken cancellationToken)
    {
        var session = await GetOrCreateSessionAsync(sessionId, cancellationToken);
        return await _runner.RunAsync(prompt, session, cancellationToken);
    }

    private async Task<object> GetOrCreateSessionAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        if (_sessions.TryGetValue(sessionId, out var existingSession))
            return existingSession;

        var newSession = await _runner.CreateSessionAsync(cancellationToken);
        return _sessions.GetOrAdd(sessionId, newSession);
    }

    private static AIAgent CreateAgent(string apiKey, string endpoint, string model)
    {
        IChatClient chatClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = new Uri(endpoint)
            })
            .GetChatClient(model)
            .AsIChatClient();

        return chatClient.AsHarnessAgent(new HarnessAgentOptions
        {
            ChatOptions = new ChatOptions
            {
                Instructions = "You are a customer support resolution assistant. " +
                    "Understand the issue, ask focused questions, and provide safe, clear resolution steps."
            },
            DisableWebSearch = true
        });
    }
}

internal sealed class HarnessAgentRunner(AIAgent agent) : IAgentRunner
{
    public async Task<object> CreateSessionAsync(CancellationToken cancellationToken) =>
        await agent.CreateSessionAsync(cancellationToken);

    public async Task<string> RunAsync(
        string prompt,
        object session,
        CancellationToken cancellationToken)
    {
        var response = await agent.RunAsync(
            prompt,
            (AgentSession)session,
            cancellationToken: cancellationToken);

        return response.Text ?? string.Empty;
    }
}