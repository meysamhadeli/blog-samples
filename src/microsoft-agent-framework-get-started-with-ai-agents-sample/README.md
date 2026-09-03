# Microsoft Agent Framework Get Started sample

This sample exposes a small HTTP API backed by a Microsoft Agent Framework `AIAgent`. It demonstrates the first building block in the series: receive a prompt and return an agent response.

## Run

Set the `DS_KEY` environment variable, then run the sample with Aspire:

```powershell
$env:DS_KEY = "<your-deepseek-api-key>"
dotnet run --project src/Aspire/AgentGetStarted.AppHost
```

Aspire starts the API at `http://localhost:5000`. You can also run the API directly with `dotnet run --project src/AgentGetStarted.Api`.

The sample uses DeepSeek's OpenAI-compatible endpoint (`https://api.deepseek.com`) and `deepseek-v4-flash` by default. Change `DeepSeek:Endpoint` and `DeepSeek:Model` when using another compatible deployment.

Send a request:

```bash
curl -X POST http://localhost:5000/api/agent/chat \
  -H "Content-Type: application/json" \
  -d '{"prompt":"I tried to pay for my order twice. Both attempts failed, but I can see a pending charge. What should I do?"}'
```

## Tests

Run unit and API integration tests:

```bash
dotnet test --solution agent-get-started-sample.slnx --no-restore
```

The API integration tests host the API in-process and use the real DeepSeek agent. The shared fixture requires `DS_KEY`, and tests verify request validation plus a non-empty response from the live provider.
