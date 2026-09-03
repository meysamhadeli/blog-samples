using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AgentTools;

public sealed record OrderStatus(string OrderId, string Status, string PaymentStatus);

public sealed class OrderStore
{
    private static readonly IReadOnlyDictionary<string, OrderStatus> Orders =
        new Dictionary<string, OrderStatus>(StringComparer.OrdinalIgnoreCase)
        {
            ["ORD-1001"] = new("ORD-1001", "Payment review", "Pending charge")
        };

    public OrderStatus GetOrderStatus(string orderId) =>
        Orders.TryGetValue(orderId.Trim(), out var order)
            ? order
            : new OrderStatus(orderId, "Unknown", "Unknown");
}

public interface ISupportKnowledgeBase
{
    Task<string> SearchAsync(string query, CancellationToken cancellationToken = default);
}

public sealed class SupportKnowledgeBase : ISupportKnowledgeBase
{
    public string Search(string query) =>
        query.Contains("pending", StringComparison.OrdinalIgnoreCase) ||
        query.Contains("charge", StringComparison.OrdinalIgnoreCase) ||
        query.Contains("payment", StringComparison.OrdinalIgnoreCase)
            ? SupportGuidance.PendingCharge
            : "No matching support guidance was found.";

    public Task<string> SearchAsync(string query, CancellationToken cancellationToken = default) =>
        Task.FromResult(Search(query));
}

public sealed class QdrantSupportKnowledgeBase : ISupportKnowledgeBase
{
    private const string CollectionName = "support-knowledge-nomic";
    private const int VectorSize = 768;
    private static readonly HttpClient EmbeddingClient = new();
    private readonly QdrantClient _client;
    private readonly Uri _embeddingEndpoint;
    private readonly string _embeddingModel;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private bool _initialized;

    public QdrantSupportKnowledgeBase(
        Uri endpoint,
        Uri? embeddingEndpoint = null,
        string? embeddingModel = null)
    {
        _client = new QdrantClient(endpoint.Host, endpoint.Port == -1 ? 6334 : endpoint.Port);
        _embeddingEndpoint = new Uri(
            (embeddingEndpoint ?? new Uri("http://localhost:11434")).ToString().TrimEnd('/') + "/api/embed");
        _embeddingModel = embeddingModel ?? "nomic-embed-text";
    }

    public async Task<string> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        var results = await _client.QueryAsync(
            CollectionName,
            query: await EmbedAsync(query, cancellationToken),
            limit: 1,
            cancellationToken: cancellationToken);

        return results.Count == 0 || results[0].Score < 0.15
            ? "No matching support guidance was found."
            : results[0].Payload["text"].StringValue;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
            return;

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_initialized)
                return;

            if (!await _client.CollectionExistsAsync(CollectionName, cancellationToken))
            {
                await _client.CreateCollectionAsync(
                    CollectionName,
                    new VectorParams { Size = VectorSize, Distance = Distance.Cosine },
                    cancellationToken: cancellationToken);
            }

            await _client.UpsertAsync(
                CollectionName,
                [new PointStruct
                {
                    Id = 1,
                    Vectors = await EmbedAsync(SupportGuidance.PendingCharge, cancellationToken),
                    Payload = { ["text"] = SupportGuidance.PendingCharge }
                }],
                cancellationToken: cancellationToken);
            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    private async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken)
    {
        using var response = await EmbeddingClient.PostAsJsonAsync(
            _embeddingEndpoint,
            new { model = _embeddingModel, input = text },
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken);
        var embedding = result?.Embeddings?.FirstOrDefault();
        if (embedding is null || embedding.Length != VectorSize)
            throw new InvalidOperationException(
                $"Embedding model '{_embeddingModel}' must return {VectorSize} dimensions.");

        return embedding;
    }

    private sealed record EmbeddingResponse(
        [property: JsonPropertyName("embeddings")] float[][]? Embeddings);
}

internal static class SupportGuidance
{
    public const string PendingCharge =
        "Pending card charges can be temporary authorization holds. " +
        "Do not ask the customer to retry repeatedly. Ask them to check their bank statement " +
        "and contact payment support if the hold remains or duplicate charges appear.";
}
