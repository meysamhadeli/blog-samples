# Microsoft Agent Framework Agent Harness sample

This sample extends Parts I and II with an application-level harness for a multi-step support request:

- The harness creates and stores a plan.
- The refund tool is blocked behind a human approval request.
- Approve or reject resumes the same plan.
- Refund execution is idempotent for repeated approved callbacks.
- `SupportAgent` uses Microsoft Agent Framework `AIAgent` for optional explanations.

## Run

```powershell
$env:ASPIRE_ALLOW_UNSECURED_TRANSPORT = "true"
dotnet run --project .\src\Aspire\AgentHarness.AppHost
```

Aspire starts the API and a Redis `redis:7.4` container with a named data volume. The current sample keeps its harness state in memory so the state transitions remain easy to inspect; `APPROVAL_STORE_URL` is the integration boundary for replacing it with durable Redis storage.

Start a plan. This endpoint needs no model key because planning and approval are deterministic:

```bash
curl -X POST http://localhost:5000/api/agent/runs \
  -H "Content-Type: application/json" \
  -d '{"sessionId":"case-1001","orderId":"ORD-1001","prompt":"Refund my duplicate charge"}'
```

Approve using the returned approval ID:

```bash
curl -X POST http://localhost:5000/api/approvals/<approval-id> \
  -H "Content-Type: application/json" \
  -d '{"approved":true,"reason":"Verified duplicate charge"}'
```

Use `false` to verify that rejection never invokes the refund operation. Set `DS_KEY` to enable `/api/agent/explain` with the DeepSeek OpenAI-compatible endpoint.

## Test

```powershell
dotnet run --project .\tests\AgentHarness.UnitTests\AgentHarness.UnitTests.csproj
dotnet run --project .\tests\AgentHarness.IntegrationTests\AgentHarness.IntegrationTests.csproj
```

Unit tests cover plan creation, rejection, idempotent approval, and duplicate callbacks. API integration tests cover request validation, pending approvals, approval and rejection endpoints, and unknown approval IDs.

The harness is intentionally in memory for learning. Production code should persist plans and approvals, authorize session ownership, validate typed tool arguments, expire pending requests, and audit every decision.