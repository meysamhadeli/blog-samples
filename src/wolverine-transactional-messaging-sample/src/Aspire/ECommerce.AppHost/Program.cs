var builder = DistributedApplication.CreateBuilder(args);

var rabbitMq = builder.AddContainer("rabbitmq", "rabbitmq", "3-management")
    .WithEndpoint(5672, 5672, name: "amqp")
    .WithEndpoint(15672, 15672, name: "management");

var postgres = builder.AddContainer("postgres", "postgres", "17")
    .WithEnvironment("POSTGRES_USER", "postgres")
    .WithEnvironment("POSTGRES_PASSWORD", "postgres")
    .WithEnvironment("POSTGRES_DB", "ecommerce")
    .WithEndpoint(5432, 5432, name: "postgres");

var mongo = builder.AddContainer("mongo", "mongo", "8")
    .WithEndpoint(27017, 27017, name: "mongo");

builder.AddProject<Projects.Catalog_Api>("catalogs-api")
    .WithReference(rabbitMq)
    .WithReference(postgres)
    .WithReference(mongo);

builder.AddProject<Projects.Order_Api>("orders-api")
    .WithReference(rabbitMq)
    .WithReference(postgres);

builder.Build().Run();
