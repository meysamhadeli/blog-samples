using BuildingBlocks.Abstractions.Messages;

namespace Contracts.Messages.MessageEnvelope;

public sealed record MessageEnvelope<TMessage>(
    Guid MessageId,
    Guid CorrelationId,
    DateTime OccurredAtUtc,
    TMessage Message
) : IMessageEnvelopeMetadata where TMessage : IMessage
{
    Type IMessageEnvelopeMetadata.MessageType => typeof(TMessage);
}

public static class MessageEnvelope
{
    public static MessageEnvelope<TMessage> Create<TMessage>(
        TMessage message,
        Guid? correlationId = null,
        Guid? messageId = null,
        DateTime? occurredAtUtc = null)
        where TMessage : IMessage => new(
            messageId ?? Guid.NewGuid(),
            correlationId ?? Guid.NewGuid(),
            occurredAtUtc ?? DateTime.UtcNow,
            message);
}
