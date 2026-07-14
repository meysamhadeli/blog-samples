# Wolverine Transactional Messaging with Aspire

This sample shows a small e-commerce system built with Wolverine, .NET Aspire, PostgreSQL, MongoDB, and a selectable RabbitMQ or Kafka transport.

## What the sample demonstrates

- Wolverine publish/consume flow between `Catalog` and `Order`
- inbox-style duplicate protection in `Order`
- local processing for MongoDB read-model projection
- transport switching through configuration with explicit RabbitMQ and Kafka wiring in the APIs

## Transport selection

Both APIs default to `rabbitmq` in `appsettings.json`.

```json
{
  "Messaging": {
    "Transport": "rabbitmq"
  }
}
```

Set the value to `kafka` to switch brokers when running the APIs directly.

When running through Aspire, set `Messaging__Transport` before starting the AppHost.

PowerShell:

```powershell
$env:Messaging__Transport = "rabbitmq"
dotnet run --project src/Aspire/ECommerce.AppHost/ECommerce.AppHost.csproj
```

```powershell
$env:Messaging__Transport = "kafka"
dotnet run --project src/Aspire/ECommerce.AppHost/ECommerce.AppHost.csproj
```

## Validation

Build the sample:

```powershell
dotnet build wolverine-transactional-messaging-sample.sln
```

Run tests:

```powershell
dotnet test tests/Services/Catalog/Catalog.Tests/Catalog.Tests.csproj
dotnet test tests/Services/Order/Order.Tests/Order.Tests.csproj
```

The current tests explicitly cover both supported broker values, `rabbitmq` and `kafka`, including normalization and rejection of unsupported values. The integration tests cover Wolverine publish/consume behavior with an in-memory bus and keep the API startup wiring aligned with RabbitMQ and Kafka.

## Sample code

Upstream reference:

- https://github.com/mehdihadeli/blog-samples/tree/main/wolverine-transactional-messaging-aspire