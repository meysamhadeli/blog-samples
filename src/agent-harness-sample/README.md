# Microsoft Agent Framework Harness sample

This sample exposes a small HTTP API backed by a Microsoft Agent Framework Harness agent. The API keeps `AgentSession` instances in memory so repeated requests with the same `sessionId` preserve the conversation.

## Run

Set the `DS_KEY` environment variable, then run the API:

```powershell
$env:DS_KEY = "<your-deepseek-api-key>"
dotnet run --project AgentHarness.Api
```

The sample uses DeepSeek's OpenAI-compatible endpoint (`https://api.deepseek.com`) and `deepseek-v4-flash` by default. Change `DeepSeek:Endpoint` and `DeepSeek:Model` when using another compatible deployment.

Send multiple requests with the returned `sessionId`:

```bash
curl -X POST http://localhost:5000/api/agent/chat \
  -H "Content-Type: application/json" \
  -d '{"prompt":"A customer cannot complete checkout after a card payment. Summarize the issue and suggest safe troubleshooting steps."}'
```

The sample intentionally uses in-memory sessions. A production application should persist serialized sessions and apply authentication and authorization before allowing a caller to resume one.

## Tests

Run unit and API integration tests:

```bash
dotnet test agent-harness-sample.slnx
```

Run the real DeepSeek integration test:

```powershell
$env:DS_KEY = "<your-deepseek-api-key>"
dotnet test tests/AgentHarness.IntegrationTests/AgentHarness.IntegrationTests.csproj
```

The live-provider test skips when `DS_KEY` is unavailable. It sends a real prompt through the HTTP API and verifies a successful response, session ID, and non-empty model output. This is an integration test because it hosts the API in-process and integrates with the external DeepSeek service; a separate E2E project is unnecessary for this sample.