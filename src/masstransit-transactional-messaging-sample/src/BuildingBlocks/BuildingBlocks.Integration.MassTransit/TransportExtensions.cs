using BuildingBlocks.Integration.MassTransit.Options;
using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Integration.MassTransit;

/// <summary>Transport-level extension methods for MassTransit configuration.</summary>
public static class TransportExtensions
{
    // ── RabbitMQ ──────────────────────────────────────────────

    /// <summary>Configure message exchange name for publishing.</summary>
    public static void PublishToRabbitQueue<T>(this IRabbitMqBusFactoryConfigurator cfg, string exchangeName)
        where T : class
        => cfg.Message<T>(m => m.SetEntityName(exchangeName));

    /// <summary>Configure receive endpoint with outbox, retry, and error policies.</summary>
    public static void ReceiveEndpointWithPolicies<TDbContext, TConsumer>(
        this IRabbitMqBusFactoryConfigurator cfg, IRegistrationContext context,
        MassTransitBusOptions busOptions, string queueName,
        Action<IRabbitMqReceiveEndpointConfigurator>? configureEndpoint = null)
        where TDbContext : DbContext where TConsumer : class, IConsumer
    {
        cfg.ReceiveEndpoint(queueName, endpoint =>
        {
            endpoint.ApplyEndpointPolicies<TDbContext>(context, busOptions);
            endpoint.ConfigureConsumer<TConsumer>(context);
            configureEndpoint?.Invoke(endpoint);
        });
    }

    // ── Kafka ─────────────────────────────────────────────────

    /// <summary>Configure Kafka topic endpoint with outbox, retry, and error policies.</summary>
    public static void TopicEndpointWithPolicies<TDbContext, TMessage, TConsumer>(
        this IKafkaFactoryConfigurator kafka, IRegistrationContext context,
        MassTransitBusOptions busOptions, string topicName, string groupId,
        Action<IKafkaTopicReceiveEndpointConfigurator<Confluent.Kafka.Ignore, TMessage>>? configure = null)
        where TDbContext : DbContext where TMessage : class where TConsumer : class, IConsumer<TMessage>
    {
        kafka.TopicEndpoint<TMessage>(topicName, groupId, endpoint =>
        {
            endpoint.ApplyEndpointPolicies<TDbContext>(context, busOptions);
            endpoint.ConfigureConsumer<TConsumer>(context);
            configure?.Invoke(endpoint);
        });
    }

    // ── Shared endpoint policies ──────────────────────────────

    /// <summary>Apply retry, outbox, and error transport policies to an endpoint.</summary>
    public static void ApplyEndpointPolicies<TDbContext>(
        this IReceiveEndpointConfigurator endpoint,
        IRegistrationContext context,
        MassTransitBusOptions busOptions)
        where TDbContext : DbContext
    {
        var immediateRetries = busOptions.Retry.MaximumAttempts - 1;
        if (immediateRetries > 0)
            endpoint.UseMessageRetry(r => r.Immediate(immediateRetries));

        if (busOptions.Retry.UseDelayedRedelivery)
            endpoint.UseDelayedRedelivery(r =>
                r.Intervals(busOptions.Retry.DelayedRedeliveryIntervals));

        if (busOptions.UseConsumerOutbox)
            endpoint.UseEntityFrameworkOutbox<TDbContext>(context);

        if (!busOptions.UseErrorTransport)
            endpoint.DiscardFaultedMessages();
    }
}
