var builder = DistributedApplication.CreateBuilder(args);
var configPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "deployments", "config"));

var tempo = builder.AddContainer("tempo", "grafana/tempo", "2.7.0")
    .WithBindMount(Path.Combine(configPath, "tempo.yaml"), "/etc/tempo.yaml")
    .WithArgs("-config.file=/etc/tempo.yaml")
    .WithEndpoint(port: 3200, targetPort: 3200, name: "http")
    .WithEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc");
var loki = builder.AddContainer("loki", "grafana/loki", "3.3.2")
    .WithBindMount(Path.Combine(configPath, "loki-config.yaml"), "/etc/loki/local-config.yaml")
    .WithContainerRuntimeArgs("--user", "0")
    .WithArgs("-config.file=/etc/loki/local-config.yaml")
    .WithEndpoint(port: 3100, targetPort: 3100, name: "http");
var prometheus = builder.AddContainer("prometheus", "prom/prometheus", "v3.1.0")
    .WithBindMount(Path.Combine(configPath, "prometheus.yaml"), "/etc/prometheus/prometheus.yml")
    .WithArgs("--config.file=/etc/prometheus/prometheus.yml", "--web.enable-remote-write-receiver")
    .WithEndpoint(port: 9090, targetPort: 9090, name: "http");
var collector = builder.AddContainer("otel-collector", "otel/opentelemetry-collector-contrib", "0.116.1")
    .WithBindMount(Path.Combine(configPath, "otel-collector-config.yaml"), "/etc/otelcol-contrib/config.yaml")
    .WithArgs("--config=/etc/otelcol-contrib/config.yaml")
    .WithEndpoint(port: 4317, targetPort: 4317, name: "otlp-grpc")
    .WithEndpoint(port: 4318, targetPort: 4318, name: "otlp-http")
    .WaitFor(tempo)
    .WaitFor(loki)
    .WaitFor(prometheus);
var grafana = builder.AddContainer("grafana", "grafana/grafana", "11.4.0")
    .WithBindMount(Path.Combine(configPath, "grafana"), "/etc/grafana/provisioning")
    .WithEndpoint(port: 3000, targetPort: 3000, name: "http")
    .WaitFor(tempo)
    .WaitFor(loki)
    .WaitFor(prometheus);
builder.AddProject<Projects.AgentWorkflows_Api>("agent-workflows-api")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WithEnvironment("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    .WaitFor(collector);
builder.Build().Run();