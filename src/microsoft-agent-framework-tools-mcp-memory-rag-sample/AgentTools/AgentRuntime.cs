using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;

namespace AgentTools;

public interface IAgentRunner
{
    Task<string> RunAsync(string sessionId, string prompt, CancellationToken cancellationToken);
}

public sealed class AgentRuntime : IAgentRunner
{
    private readonly IAgentRunner _runner;

    public AgentRuntime(string apiKey, string endpoint, string model)
        : this(new AgentRunner(CreateAgent(apiKey, endpoint, model)))
    {
    }

    public AgentRuntime(IAgentRunner runner)
    {
        _runner = runner;
    }

    public Task<string> RunAsync(string sessionId, string prompt, CancellationToken cancellationToken) =>
        _runner.RunAsync(sessionId, prompt, cancellationToken);

    private static AIAgent CreateAgent(string apiKey, string endpoint, string model)
    {
        var mcpClient = McpClient.CreateAsync(
            new StdioClientTransport(new()
            {
                Name = "support-tools",
                Command = "dotnet",
                Arguments = ["run", "--project", ResolveMcpServerProject(), "--no-launch-profile"]
            })).GetAwaiter().GetResult();
        var tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();

        IChatClient chatClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
            .GetChatClient(model)
            .AsIChatClient();

        var agent = chatClient.AsAIAgent(
            instructions: "You are a support assistant. Use tools for order facts and support policy. " +
                "Never invent an order status or claim an action was completed. " +
                "Give safe, clear, concise answers.",
            tools: [.. tools]);

        return agent;
    }

    private static string ResolveMcpServerProject()
    {
        var configuredPath = Environment.GetEnvironmentVariable("MCP_SERVER_PROJECT");
        if (!string.IsNullOrWhiteSpace(configuredPath))
            return configuredPath;

        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                var candidate = Path.Combine(
                    directory.FullName,
                    "SupportMcpServer",
                    "SupportMcpServer.csproj");
                if (File.Exists(candidate))
                    return candidate;

                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException(
            "Could not find SupportMcpServer.csproj. Set MCP_SERVER_PROJECT to its full path.");
    }

    private sealed class AgentRunner(AIAgent agent) : IAgentRunner
    {
        private readonly Dictionary<string, AgentSession> _sessions = new(StringComparer.Ordinal);

        public async Task<string> RunAsync(
            string sessionId,
            string prompt,
            CancellationToken cancellationToken)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                session = await agent.CreateSessionAsync(cancellationToken);
                _sessions[sessionId] = session;
            }

            var response = await agent.RunAsync(
                prompt,
                session,
                cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }
    }
}
