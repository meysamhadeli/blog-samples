# Microsoft Agent Framework Workflows, Multi-Agent Systems, and Observability sample

This sample models a support-ticket workflow with two parallel specialist executors, a synthesis step, and OpenTelemetry traces and metrics.

## Run with Docker Compose

This follows the observability stack used by the existing full observability sample:

```powershell
docker compose -f .\deployments\docker-compose\docker-compose.infrastructure.yaml up -d
$env:OTEL_EXPORTER_OTLP_ENDPOINT = "http://localhost:14317"
dotnet run --project .\src\AgentWorkflows.Api
```

Open Grafana at `http://localhost:3000` with `admin` / `admin`. Collector receives OTLP, Tempo stores traces, Loki stores logs, and Prometheus stores metrics. Call the workflow:

```bash
curl -X POST http://localhost:5000/api/workflows/support -H "Content-Type: application/json" -d "{\"ticketId\":\"ticket-1\",\"text\":\"I see a duplicate charge\"}"
```

Aspire is also available and starts the same observability services:

```powershell
$env:ASPIRE_ALLOW_UNSECURED_TRANSPORT = "true"
dotnet run --project .\src\Aspire\AgentWorkflows.AppHost
```

## Tests

```powershell
dotnet run --project .\tests\AgentWorkflows.UnitTests\AgentWorkflows.UnitTests.csproj
dotnet run --project .\tests\AgentWorkflows.IntegrationTests\AgentWorkflows.IntegrationTests.csproj
```

Tests use deterministic executors and do not require an API key. Production workflows should add durable checkpoints, bounded retries, authorization, and telemetry redaction.