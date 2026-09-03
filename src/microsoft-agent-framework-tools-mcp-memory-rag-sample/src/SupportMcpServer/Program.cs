using AgentTools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
	.AddMcpServer()
	.WithStdioServerTransport()
	.WithTools<SupportTools>();

await builder.Build().RunAsync();

[McpServerToolType]
public sealed class SupportTools
{
	[McpServerTool, System.ComponentModel.Description("Read the current order and payment status.")]
	public static OrderStatus GetOrderStatus(string orderId) =>
		new OrderStore().GetOrderStatus(orderId);

	[McpServerTool, System.ComponentModel.Description("Search approved support guidance for the customer question.")]
	public static Task<string> SearchSupportKnowledge(
		string query,
		CancellationToken cancellationToken = default)
	{
		var endpoint = Environment.GetEnvironmentVariable("QDRANT_URL");
		ISupportKnowledgeBase knowledge = string.IsNullOrWhiteSpace(endpoint)
			? new SupportKnowledgeBase()
			: new QdrantSupportKnowledgeBase(
				new Uri(endpoint),
				new Uri(Environment.GetEnvironmentVariable("EMBEDDING_URL") ?? "http://localhost:11434"),
				Environment.GetEnvironmentVariable("EMBEDDING_MODEL") ?? "nomic-embed-text");

		return knowledge.SearchAsync(query, cancellationToken);
	}
}
