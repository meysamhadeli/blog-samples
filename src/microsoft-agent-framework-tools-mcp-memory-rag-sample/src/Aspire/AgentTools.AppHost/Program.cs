const string QdrantImage = "qdrant/qdrant";
const string QdrantTag = "v1.19.0";
const string OllamaImage = "ollama/ollama";
const string OllamaTag = "0.33.1";
const string EmbeddingModel = "nomic-embed-text";

var builder = DistributedApplication.CreateBuilder(args);

var qdrant = builder
    .AddContainer("qdrant", QdrantImage, QdrantTag)
    .WithEndpoint(port: 6333, targetPort: 6333, name: "http")
    .WithEndpoint(port: 6334, targetPort: 6334, name: "grpc")
    .WithVolume("qdrant_storage", "/qdrant/storage");

var ollama = builder
    .AddContainer("ollama", OllamaImage, OllamaTag)
    .WithHttpEndpoint(port: 11500, targetPort: 11434, name: "http")
    .WithVolume("ollama_storage", "/root/.ollama");

var modelInit = builder
    .AddContainer("ollama-model-init", OllamaImage, OllamaTag)
    .WithEntrypoint("/bin/sh")
    .WithArgs("-c", $"ollama pull {EmbeddingModel}")
    .WithEnvironment("OLLAMA_HOST", "http://ollama:11434")
    .WithVolume("ollama_storage", "/root/.ollama")
    .WaitFor(ollama);

builder
    .AddProject<Projects.AgentTools_Api>("agent-tools-api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithEnvironment("QDRANT_URL", "http://localhost:6334")
    .WithEnvironment("EMBEDDING_URL", "http://localhost:11500")
    .WithEnvironment("EMBEDDING_MODEL", EmbeddingModel)
    .WithEnvironment(
        "MCP_SERVER_PROJECT",
        Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "SupportMcpServer", "SupportMcpServer.csproj")))
    .WaitFor(qdrant)
    .WaitForCompletion(modelInit);

builder.Build().Run();