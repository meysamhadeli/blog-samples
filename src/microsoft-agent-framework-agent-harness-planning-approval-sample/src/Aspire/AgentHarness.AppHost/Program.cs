const string RedisImage = "redis";
const string RedisTag = "7.4";

var builder = DistributedApplication.CreateBuilder(args);

var approvalStore = builder
    .AddContainer("approval-store", RedisImage, RedisTag)
    .WithEndpoint(port: 6379, targetPort: 6379, name: "redis")
    .WithVolume("approval_store_data", "/data");

builder
    .AddProject<Projects.AgentHarness_Api>("agent-harness-api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithEnvironment("APPROVAL_STORE_URL", "redis://localhost:6379")
    .WaitFor(approvalStore);

builder.Build().Run();