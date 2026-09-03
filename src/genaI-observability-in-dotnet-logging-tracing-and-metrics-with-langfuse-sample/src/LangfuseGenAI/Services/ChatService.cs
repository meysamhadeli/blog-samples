using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using zborek.Langfuse.OpenTelemetry.Trace;

namespace LangfuseGenAI.Services;

/// <summary>
/// Weather plugin — Semantic Kernel function that the LLM calls as a tool.
/// Each invocation emits its own OTel child span via a dedicated ActivitySource,
/// visible as a separate observation in the Langfuse trace.
/// </summary>
public sealed class WeatherPlugin
{
    private static readonly ActivitySource ActivitySource = new("LangfuseGenAI.WeatherPlugin");

    [KernelFunction("get_weather")]
    [Description("Get the current weather for a city")]
    public string GetWeather(string city)
    {
        using var activity = ActivitySource.StartActivity("get_weather", ActivityKind.Internal);
        activity?.SetTag("tool.name", "get_weather");
        activity?.SetTag("tool.city", city);

        var result = city switch
        {
            "Tokyo" => "Sunny, 22°C",
            "Paris" => "Rainy, 15°C",
            "New York" => "Cloudy, 18°C",
            "London" => "Foggy, 12°C",
            _ => "Sunny, 20°C"
        };

        activity?.SetTag("tool.weather", result);

        return result;
    }
}

public sealed class ChatService(
    Kernel kernel,
    ILogger<ChatService> logger,
    IServiceProvider serviceProvider)
{
    private static readonly Meter ChatMeter = new("LangfuseGenAI.Meters");
    private static readonly Counter<int> ChatRequestCounter =
        ChatMeter.CreateCounter<int>("genai.chat.requests", description: "Total chat requests");
    private static readonly Histogram<double> ChatLatency =
        ChatMeter.CreateHistogram<double>("genai.chat.latency_ms", description: "Chat request latency in ms");
    private static readonly Counter<int> TokenCounter =
        ChatMeter.CreateCounter<int>("genai.chat.tokens", description: "Total tokens used");

    public async Task<string> ProcessChatAsync(
        string userMessage,
        string? sessionId = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        ChatRequestCounter.Add(1);

        serviceProvider.GetService<IOtelLangfuseTrace>()?.SetPublic(true);

        logger.LogInformation(
            "Processing chat request. SessionId: {SessionId}, UserId: {UserId}",
            sessionId, userId);

        try
        {
            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(
                "You are a weather assistant. For any weather question, you must call the get_weather tool for each requested city and use the tool results in your answer. Do not guess weather.");
            chatHistory.AddUserMessage(userMessage);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.7f,
                MaxTokens = 1000,
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            logger.LogDebug("Sending request to DeepSeek model: deepseek-chat");

            // SK handles the full tool-calling loop internally with AutoInvokeKernelFunctions.
            // Each tool/function call emits an OTel child activity via SK's built-in
            // instrumentation — visible as separate observations in Langfuse.
            var result = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                executionSettings: executionSettings,
                kernel: kernel,
                cancellationToken: cancellationToken);

            sw.Stop();
            ChatLatency.Record(sw.ElapsedMilliseconds);

            var outputTokens = result.Content?.Length / 4 ?? 0;
            TokenCounter.Add(outputTokens);

            logger.LogInformation(
                "Chat completed in {ElapsedMs}ms. Output tokens (est): {OutputTokens}",
                sw.ElapsedMilliseconds, outputTokens);

            return result.Content ?? string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chat request failed for session {SessionId}", sessionId);
            throw;
        }
    }
}
