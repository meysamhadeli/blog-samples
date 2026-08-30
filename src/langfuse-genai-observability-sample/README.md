# Langfuse GenAI Observability Sample

A .NET sample demonstrating GenAI observability with Langfuse, using **Microsoft.SemanticKernel** for OpenAI-compatible chat completions and tool calling.

## Stack

- .NET 10.0
- ASP.NET Core
- Microsoft.SemanticKernel
- DeepSeek (OpenAI-compatible API)
- LangfuseDotnet (OpenTelemetry-based observability)
- OpenTelemetry (traces, metrics, logs)
- Docker Compose (local Langfuse stack)

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) or Docker Engine + Compose
- DeepSeek API key (or any OpenAI-compatible provider)

## Setup

1. Start Langfuse locally with Docker Compose:

```powershell
docker compose up -d
```

This starts Postgres, ClickHouse, MinIO, Redis, the Langfuse worker, and the Langfuse web UI on `http://localhost:3001`. The compose file seeds a default project and account:

- **Email**: `admin@example.com`
- **Password**: `LangfuseDemo2026!`

The sample's `appsettings.json` already points to the seeded local project, so no manual key configuration is needed. The compose file sets both `LANGFUSE_INIT_ORG_ID` and `LANGFUSE_INIT_PROJECT_ID`; without these IDs Langfuse skips the seeding step.

2. Set your AI provider API key:

```powershell
$env:APP_API_KEY = "sk-..."
```

The sample defaults to DeepSeek (`https://api.deepseek.com`, model `deepseek-chat`). Override via `appsettings.json` or environment variables if you use another provider.

Langfuse credentials are optional. The API checks for the `Langfuse:PublicKey` and `Langfuse:SecretKey` settings at startup. If they are missing, the Langfuse exporter is not registered and the API still runs, but no traces are sent.

3. Run the application:

```powershell
dotnet run --project src/LangfuseGenAI
```

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/v1/chat` | Chat completion with weather tool calling |

Example request:

```json
POST http://localhost:5000/api/v1/chat
Content-Type: application/json

{
  "message": "What is the weather in Tokyo?",
  "sessionId": "demo-session",
  "userId": "demo-user"
}
```

## Observability

The sample sends OpenTelemetry traces to Langfuse for LLM observability:

- **Traces**: Chat completions with nested generation and tool-call spans
- **Metrics**: Request count, latency histogram, token usage
- **Logs**: Structured logs correlated with trace IDs

Open the Langfuse UI at `http://localhost:3001`, sign in with the seeded account, and inspect traces, observations, sessions, and users.
