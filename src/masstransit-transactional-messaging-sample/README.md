# MassTransit Transactional Messaging with Aspire

This sample shows a small e-commerce system built with MassTransit, .NET Aspire, PostgreSQL, MongoDB, and a selectable RabbitMQ or Kafka transport.

## What the sample demonstrates

- transactional outbox style publishing from `Catalog`
- durable inbox style duplicate protection in `Order`
- durable local processing for MongoDB read-model projection
- transport switching through configuration with explicit RabbitMQ and Kafka wiring in the APIs

The sample keeps the same business flow as the Wolverine version, but now uses MassTransit abstractions in the application flow and validates publish/consume behavior with integration tests.

## Transport selection

Both APIs default to `rabbitmq` in `appsettings.json`.

```json
{
  "Messaging": {
    "Transport": "rabbitmq"
  }
}
```

Set the value to `kafka` to switch the configuration value when running the APIs directly.

Current implementation detail:

- `rabbitmq` uses MassTransit RabbitMQ transport in both APIs
- `kafka` uses a MassTransit Kafka rider with a producer in `Catalog` and a topic endpoint in `Order`

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
dotnet build masstransit-transactional-messaging-sample.sln
```

Run tests:

```powershell
dotnet test tests/Services/Catalog/Catalog.Tests/Catalog.Tests.csproj
dotnet test tests/Services/Order/Order.Tests/Order.Tests.csproj
```

The current tests explicitly cover both supported configuration values, `rabbitmq` and `kafka`. They validate transport normalization and supported values, and they verify publish/consume behavior through a real in-memory MassTransit bus-backed setup.

The API projects are wired for both live RabbitMQ and live Kafka. The automated tests do not start external brokers; they keep broker-independent behavior covered with the in-memory MassTransit bus.

## Sample code

Blog sample reference:

- https://github.com/meysamhadeli/blog-samples/tree/main/src/masstransit-transactional-messaging-sample