# Microsoft Agent Framework Tools, Memory, and RAG sample

This sample extends the Part I agent with two read-only MCP tools, conversation sessions, and Qdrant-backed vector retrieval:

- `SupportMcpServer` publishes `get_order_status` and `search_support_knowledge` over stdio.
- The API discovers those tools through MCP instead of registering them in-process.
- `search_support_knowledge` retrieves approved pending-charge guidance from Qdrant.
- Ollama runs `nomic-embed-text` locally for semantic embeddings; no embedding API key is required.

## Run

```powershell
$env:DS_KEY = "<your-deepseek-api-key>"
$env:ASPIRE_ALLOW_UNSECURED_TRANSPORT = "true"
dotnet run --project .\src\Aspire\AgentTools.AppHost
```

Aspire starts Qdrant and Ollama, then pulls `nomic-embed-text` automatically through the one-shot `ollama-model-init` resource. When QDRANT_URL is not set, the MCP server uses an in-memory fallback. When Qdrant is enabled, Ollama and the model must be running because the sample uses real 768-dimensional semantic embeddings.

Send a real-world support request:

```bash
curl -X POST http://localhost:5000/api/agent/chat \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"case-1001","prompt":"My order ORD-1001 failed twice, but I see a pending charge. Is my order confirmed?"}'
```

Reuse `case-1001` for a follow-up question. The sample stores sessions in memory for learning purposes.

## Tests

```bash
dotnet test --solution agent-tools-mcp-rag-sample.slnx --no-restore
```

Unit tests cover deterministic order and knowledge lookup. Integration tests use Testcontainers to start Qdrant and Ollama, pull `nomic-embed-text`, and host the real API, MCP server, DeepSeek agent, and retrieval path. Docker must be running, and the tests require `DS_KEY`.
