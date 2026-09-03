using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Tests.Shared.Fixtures;

public sealed class QdrantOllamaContainerFixture : IAsyncLifetime
{
    private const string QdrantImage = "qdrant/qdrant:v1.19.0";
    private const string OllamaImage = "ollama/ollama:0.33.1";
    private const string EmbeddingModel = "nomic-embed-text";

    private readonly IContainer _qdrant = new ContainerBuilder()
        .WithImage(QdrantImage)
        .WithPortBinding(6334, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(6334))
        .Build();

    private readonly IContainer _ollama = new ContainerBuilder()
        .WithImage(OllamaImage)
        .WithPortBinding(11434, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(11434))
        .Build();

    public string QdrantUrl => $"http://localhost:{_qdrant.GetMappedPublicPort(6334)}";

    public string EmbeddingUrl => $"http://localhost:{_ollama.GetMappedPublicPort(11434)}";

    public async ValueTask InitializeAsync()
    {
        await _qdrant.StartAsync();
        await _ollama.StartAsync();

        var result = await _ollama.ExecAsync(["ollama", "pull", EmbeddingModel]);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"Could not pull Ollama model '{EmbeddingModel}': {result.Stderr}");
    }

    public async ValueTask DisposeAsync()
    {
        await _ollama.DisposeAsync();
        await _qdrant.DisposeAsync();
    }
}