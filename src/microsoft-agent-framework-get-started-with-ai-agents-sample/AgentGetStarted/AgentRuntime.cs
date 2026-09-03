using System.ClientModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AgentGetStarted;

public interface IAgentRunner
{
    Task<string> RunAsync(string prompt, CancellationToken cancellationToken);
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

    public Task<string> RunAsync(string prompt, CancellationToken cancellationToken) =>
        _runner.RunAsync(prompt, cancellationToken);

    private static AIAgent CreateAgent(string apiKey, string endpoint, string model)
    {
        IChatClient chatClient = new OpenAIClient(
                new ApiKeyCredential(apiKey),
                new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
            .GetChatClient(model)
            .AsIChatClient();

        return chatClient.AsAIAgent(
            instructions: "You are a customer support assistant. " +
                "Give safe, clear, concise troubleshooting steps. " +
                "Do not claim to access orders, payments, or customer accounts. " +
                "For possible duplicate charges, advise the customer to avoid repeated payment attempts, " +
                "check their bank statement, and contact the payment-support team with their order information.");
    }

    private sealed class AgentRunner(AIAgent agent) : IAgentRunner
    {
        public async Task<string> RunAsync(
            string prompt,
            CancellationToken cancellationToken)
        {
            var response = await agent.RunAsync(prompt, cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }
    }
}
