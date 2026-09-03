var builder = DistributedApplication.CreateBuilder(args);
var apiKey = Environment.GetEnvironmentVariable("DS_KEY")
    ?? throw new InvalidOperationException("Configure the DS_KEY environment variable before starting Aspire.");

builder.AddProject<Projects.AgentGetStarted_Api>("agent-get-started-api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithEnvironment("DS_KEY", apiKey);

builder.Build().Run();
