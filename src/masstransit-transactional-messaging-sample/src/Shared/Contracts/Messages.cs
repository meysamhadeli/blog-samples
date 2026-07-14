namespace Contracts;

public sealed record MessageEnvelope<T>(
    Guid MessageId,
    DateTime CreatedAt,
    string CorrelationId,
    T Data)
{
    public static MessageEnvelope<T> Create(T data, string? correlationId = null)
    {
        return new MessageEnvelope<T>(
            Guid.NewGuid(),
            DateTime.UtcNow,
            correlationId ?? Guid.NewGuid().ToString("N"),
            data);
    }
}

public sealed record ProductCreatedV1(Guid ProductId, string Name, decimal Price, int Stock);

public sealed record ProjectProductReadModel(Guid ProductId);

public sealed record MessagingOptions
{
    public string Transport { get; init; } = "rabbitmq";
}

public static class MessagingTransport
{
    public const string RabbitMq = "rabbitmq";
    public const string Kafka = "kafka";

    public static bool IsSupported(string? transport)
    {
        return string.Equals(transport, RabbitMq, StringComparison.OrdinalIgnoreCase)
            || string.Equals(transport, Kafka, StringComparison.OrdinalIgnoreCase);
    }

    public static string Normalize(string? transport)
    {
        var normalized = transport?.Trim().ToLowerInvariant();

        if (!IsSupported(normalized))
        {
            throw new InvalidOperationException($"Unsupported messaging transport '{transport}'. Supported values are '{RabbitMq}' and '{Kafka}'.");
        }

        return normalized!;
    }
}
