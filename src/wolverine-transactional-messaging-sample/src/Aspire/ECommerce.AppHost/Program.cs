var builder = DistributedApplication.CreateBuilder(args);

var transport = builder.Configuration["Messaging:Transport"]?.Trim().ToLowerInvariant() ?? "rabbitmq";

var rabbitMq = builder.AddContainer("rabbitmq", "rabbitmq", "3-management")
    .WithEndpoint(5672, 5672, name: "amqp")
    .WithEndpoint(15672, 15672, name: "management");

var kafka = builder.AddContainer("kafka", "apache/kafka", "latest")
    .WithEnvironment("KAFKA_NODE_ID", "1")
    .WithEnvironment("KAFKA_PROCESS_ROLES", "broker,controller")
    .WithEnvironment("KAFKA_LISTENERS", "PLAINTEXT://:9092,CONTROLLER://:9093")
    .WithEnvironment("KAFKA_ADVERTISED_LISTENERS", "PLAINTEXT://kafka:9092")
    .WithEnvironment("KAFKA_CONTROLLER_LISTENER_NAMES", "CONTROLLER")
    .WithEnvironment("KAFKA_LISTENER_SECURITY_PROTOCOL_MAP", "PLAINTEXT:PLAINTEXT,CONTROLLER:PLAINTEXT")
    .WithEnvironment("KAFKA_CONTROLLER_QUORUM_VOTERS", "1@localhost:9093")
    .WithEnvironment("KAFKA_INTER_BROKER_LISTENER_NAME", "PLAINTEXT")
    .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
    .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
    .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
    .WithEnvironment("KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS", "0")
    .WithEnvironment("KAFKA_NUM_PARTITIONS", "1")
    .WithEndpoint(9092, 9092, name: "kafka");

var postgres = builder.AddContainer("postgres", "postgres", "17")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEnvironment("POSTGRES_DB", "ecommerce")
    .WithEndpoint(5432, 5432, name: "postgres");

var mongo = builder.AddContainer("mongo", "mongo", "8")
    .WithEndpoint(27017, 27017, name: "mongo");

var catalogsApi = builder.AddProject<Projects.Catalog_Api>("catalogs-api")
    .WithEnvironment("Messaging__Transport", transport)
    .WithReference(postgres)
    .WithReference(mongo);

var ordersApi = builder.AddProject<Projects.Order_Api>("orders-api")
    .WithEnvironment("Messaging__Transport", transport)
    .WithReference(postgres);

switch (transport)
{
    case "rabbitmq":
        catalogsApi.WithReference(rabbitMq);
        ordersApi.WithReference(rabbitMq);
        break;
    case "kafka":
        catalogsApi.WithReference(kafka);
        ordersApi.WithReference(kafka);
        break;
    default:
        throw new InvalidOperationException($"Unsupported messaging transport '{transport}'. Supported values are 'rabbitmq' and 'kafka'.");
}

builder.Build().Run();
