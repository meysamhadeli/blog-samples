# Microsoft Agent Framework Get Started sample

This sample exposes a small HTTP API backed by a Microsoft Agent Framework `AIAgent`. It demonstrates the first building block in the series: receive a prompt and return an agent response.

## Run

Set the `DS_KEY` environment variable, then run the API:

```powershell
$env:DS_KEY = "<your-deepseek-api-key>"
dotnet run --project AgentGetStarted.Api
```

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
dotnet test agent-get-started-sample.slnx
```

The API integration tests replace the model runner with a fake, so request validation and HTTP behavior do not require credentials. The live-provider fixture automatically checks `DS_KEY` during setup and skips its test when the key is unavailable. The live test hosts the API in-process and verifies that the real agent returns non-empty output.
