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
