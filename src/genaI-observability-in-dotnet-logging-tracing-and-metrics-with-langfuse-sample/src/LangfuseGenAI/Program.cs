using LangfuseGenAI.Endpoints;
using LangfuseGenAI.Services;
using Microsoft.SemanticKernel;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using zborek.Langfuse.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

// ---- OpenTelemetry with Langfuse Exporter ----
var serviceVersion = "1.0.0";

var langfuseSection = builder.Configuration.GetSection("Langfuse");
var hasLangfuseKeys = !string.IsNullOrEmpty(langfuseSection["PublicKey"])
    && !string.IsNullOrEmpty(langfuseSection["SecretKey"]);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "LangfuseGenAI",
            serviceVersion: serviceVersion,
            serviceInstanceId: Environment.MachineName
        )
    )
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource("LangfuseGenAI.WeatherPlugin");

        if (hasLangfuseKeys)
            tracing.AddLangfuseExporter(langfuseSection);
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("LangfuseGenAI.Meters");
    });

// Register IOtelLangfuseTrace for DI (only if Langfuse keys present)
if (hasLangfuseKeys)
    builder.Services.AddLangfuseTracing();

// ---- Semantic Kernel (DeepSeek via OpenAI-compatible API) ----
var aiConfig = builder.Configuration.GetSection("AI");

var apiKey = Environment.GetEnvironmentVariable("APP_API_KEY")
    ?? builder.Configuration["AI:ApiKey"]
    ?? throw new InvalidOperationException("AI API key is required. Set APP_API_KEY env var or configure AI:ApiKey.");

var endpoint = aiConfig["Endpoint"] ?? "https://api.deepseek.com";
var chatModel = aiConfig["ChatModel"] ?? "deepseek-chat";

// Register Semantic Kernel with OpenAI-compatible DeepSeek chat completion
var kernelBuilder = builder.Services.AddKernel();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: chatModel,
    apiKey: apiKey,
    endpoint: new Uri(endpoint));

// Register weather plugin — each tool call emits an OTel span visible in Langfuse
kernelBuilder.Plugins.AddFromType<WeatherPlugin>();

// ---- Application Services ----
builder.Services.AddScoped<ChatService>();

// ---- Swagger / OpenAPI ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// ---- Endpoints ----
app.MapChatEndpoints();

app.Run();
