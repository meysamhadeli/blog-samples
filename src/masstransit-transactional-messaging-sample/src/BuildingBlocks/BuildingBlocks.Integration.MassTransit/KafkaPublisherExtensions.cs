using BuildingBlocks.Integration.MassTransit.Abstractions;
using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Integration.MassTransit;

public static class KafkaPublisherExtensions
{
    public static IServiceCollection AddKafkaMessagePublisher<TMessage>(
        this IServiceCollection services)
        where TMessage : class
    {
        services.AddScoped<IMassTransitMessagePublisher, KafkaTopicMessagePublisher<TMessage>>();
        return services;
    }
}

internal sealed class KafkaTopicMessagePublisher<TMessage>(ITopicProducer<TMessage> producer)
    : IMassTransitMessagePublisher
    where TMessage : class
{
    public Task PublishAsync<TRequestedMessage>(
        TRequestedMessage message,
        CancellationToken cancellationToken)
        where TRequestedMessage : class
    {
        if (message is not TMessage typedMessage)
        {
            throw new InvalidOperationException(
                $"Kafka publisher does not support message type '{typeof(TRequestedMessage).Name}'. Expected '{typeof(TMessage).Name}'.");
        }
        return producer.Produce(typedMessage, cancellationToken);
    }
}
