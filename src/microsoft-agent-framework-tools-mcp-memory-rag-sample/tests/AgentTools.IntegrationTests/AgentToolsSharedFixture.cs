using Tests.Shared.Fixtures;

namespace AgentTools.IntegrationTests;

public sealed class AgentToolsSharedFixture : SharedFixture<Program>
{
    public QdrantOllamaContainerFixture Infrastructure { get; } = new();

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Infrastructure.InitializeAsync();
        Environment.SetEnvironmentVariable("QDRANT_URL", Infrastructure.QdrantUrl);
        Environment.SetEnvironmentVariable("EMBEDDING_URL", Infrastructure.EmbeddingUrl);
        Environment.SetEnvironmentVariable("EMBEDDING_MODEL", "nomic-embed-text");
        RequireDeepSeekApiKey();
    }

    public override async ValueTask DisposeAsync()
    {
        await Infrastructure.DisposeAsync();
        await base.DisposeAsync();
    }
}
