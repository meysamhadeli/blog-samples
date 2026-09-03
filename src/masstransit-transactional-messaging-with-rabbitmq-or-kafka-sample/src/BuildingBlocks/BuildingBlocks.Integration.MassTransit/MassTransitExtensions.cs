using System.Reflection;
using BuildingBlocks.Abstractions.Messages;
using BuildingBlocks.Integration.MassTransit.Abstractions;
using BuildingBlocks.Integration.MassTransit.Configuration;
using BuildingBlocks.Integration.MassTransit.Options;
using MassTransit;
using MassTransit.KafkaIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Integration.MassTransit;

/// <summary>
/// Single entry point for all MassTransit configuration.
/// Replaces AddMassTransitMessaging, AddMassTransitRabbitMq, AddMassTransitKafka,
/// AddDurableLocalQueue, and AddDurableCommandHandler.
/// </summary>
public static class MassTransitExtensions
{
    public static IServiceCollection AddTransactionalMassTransit<TDbContext>(
        this IServiceCollection services,
        MassTransitOptions options,
        MessagingTransportType transport,
        Action<TransactionalMassTransitConfigurator<TDbContext>> configure,
        IHostEnvironment? environment = null)
        where TDbContext : DbContext
    {
        var configurator = new TransactionalMassTransitConfigurator<TDbContext>(options, transport, services, environment);
        configure(configurator);
        return configurator.Apply();
    }
}

public class TransactionalMassTransitConfigurator<TDbContext>
    where TDbContext : DbContext
{
    private readonly MassTransitOptions _options;
    private readonly MessagingTransportType _transport;
    private readonly IServiceCollection _services;
    private readonly IHostEnvironment? _environment;

    private Action<IBusRegistrationConfigurator>? _configureBus;
    private Action<IServiceCollection>? _configureServices;

    // RabbitMQ-specific
    private Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? _configureRabbitMq;

    // Kafka-specific
    private Action<IRiderRegistrationConfigurator>? _configureKafkaRider;
    private Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? _configureKafkaTransport;
    private Action<IServiceCollection>? _configureKafkaPublisher;

    internal TransactionalMassTransitConfigurator(
        MassTransitOptions options,
        MessagingTransportType transport,
        IServiceCollection services,
        IHostEnvironment? environment)
    {
        _options = options;
        _transport = transport;
        _services = services;
        _environment = environment;
    }

    /// <summary>Register consumers on the bus.</summary>
    public TransactionalMassTransitConfigurator<TDbContext> AddConsumer<TConsumer>()
        where TConsumer : class, IConsumer
    {
        _configureBus += x => x.AddConsumer<TConsumer>();
        return this;
    }

    /// <summary>Register consumers with a custom definition.</summary>
    public TransactionalMassTransitConfigurator<TDbContext> AddConsumer<TConsumer, TDefinition>()
        where TConsumer : class, IConsumer
        where TDefinition : ConsumerDefinition<TConsumer>
    {
        _configureBus += x => x.AddConsumer<TConsumer, TDefinition>();
        return this;
    }

    /// <summary>Configure RabbitMQ transport (only used when transport is RabbitMQ).</summary>
    public TransactionalMassTransitConfigurator<TDbContext> UseRabbitMq(
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator> configureTransport)
    {
        _configureRabbitMq = configureTransport;
        return this;
    }

    /// <summary>Configure Kafka transport (only used when transport is Kafka).</summary>
    public TransactionalMassTransitConfigurator<TDbContext> UseKafka(
        Action<IRiderRegistrationConfigurator>? configureRider = null,
        Action<IRiderRegistrationContext, IKafkaFactoryConfigurator>? configureTransport = null,
        Action<IServiceCollection>? configurePublisher = null)
    {
        _configureKafkaRider = configureRider;
        _configureKafkaTransport = configureTransport;
        _configureKafkaPublisher = configurePublisher;
        return this;
    }

    /// <summary>Configure custom bus registration.</summary>
    public TransactionalMassTransitConfigurator<TDbContext> ConfigureBus(Action<IBusRegistrationConfigurator> configure)
    {
        _configureBus += configure;
        return this;
    }

    /// <summary>Configure custom services.</summary>
    public TransactionalMassTransitConfigurator<TDbContext> ConfigureServices(Action<IServiceCollection> configure)
    {
        _configureServices += configure;
        return this;
    }

    /// <summary>
    /// Scan assemblies for <see cref="IIntegrationEvent"/> types and auto-register
    /// transport wiring (RabbitMQ queue publishing, Kafka producer + publisher).
    /// <para>
    /// Routing name is derived from the assembly name (last segment, kebab-cased)
    /// plus the type name (version suffix stripped, kebab-cased).
    /// Example: assembly <c>ECommerce.Services.Catalogs</c>, type <c>ProductCreatedV1</c>
    /// → <c>catalogs-product-created</c>.
    /// </para>
    /// </summary>
    public TransactionalMassTransitConfigurator<TDbContext> ScanIntegrationEvents(
        params Assembly[] assemblies)
    {
        var scanned = new List<(Type EventType, string Name)>();

        foreach (var assembly in assemblies)
        {
            var modulePrefix = DeriveModulePrefix(assembly);

            foreach (var eventType in assembly.GetTypes().Where(t =>
                         t is { IsAbstract: false, IsInterface: false }
                         && typeof(IIntegrationEvent).IsAssignableFrom(t)))
            {
                var name = $"{modulePrefix}-{DeriveEventName(eventType.Name)}";
                scanned.Add((eventType, name));
            }
        }

        if (scanned.Count == 0) return this;

        if (_transport is MessagingTransportType.RabbitMq)
        {
            var prev = _configureRabbitMq;
            _configureRabbitMq = (ctx, rabbit) =>
            {
                prev?.Invoke(ctx, rabbit);
                foreach (var (eventType, queueName) in scanned)
                {
                    var envelopeType = FindEnvelopeType(eventType);
                    typeof(TransportExtensions)
                        .GetMethod(nameof(TransportExtensions.PublishToRabbitQueue))!
                        .MakeGenericMethod(envelopeType)
                        .Invoke(null, [rabbit, queueName]);
                }
            };
        }

        if (_transport is MessagingTransportType.Kafka)
        {
            var prevRider = _configureKafkaRider;
            _configureKafkaRider = rider =>
            {
                prevRider?.Invoke(rider);
                foreach (var (eventType, topicName) in scanned)
                {
                    var envelopeType = FindEnvelopeType(eventType);
                    typeof(KafkaProducerRegistrationExtensions)
                        .GetMethod("AddProducer", [typeof(IRiderRegistrationConfigurator), typeof(string)])!
                        .MakeGenericMethod(envelopeType)
                        .Invoke(null, [rider, topicName]);
                }
            };

            var prevPublisher = _configureKafkaPublisher;
            _configureKafkaPublisher = services =>
            {
                prevPublisher?.Invoke(services);
                foreach (var (eventType, _) in scanned)
                {
                    var envelopeType = FindEnvelopeType(eventType);
                    typeof(KafkaPublisherExtensions)
                        .GetMethod(nameof(KafkaPublisherExtensions.AddKafkaMessagePublisher))!
                        .MakeGenericMethod(envelopeType)
                        .Invoke(null, [services]);
                }
            };
        }

        return this;
    }

    /// <summary>
    /// Extract module prefix from assembly name.
    /// <c>ECommerce.Services.Catalogs</c> → <c>catalogs</c>.
    /// </summary>
    private static string DeriveModulePrefix(Assembly assembly)
    {
        var name = assembly.GetName().Name!;
        var lastDot = name.LastIndexOf('.');
        return lastDot >= 0
            ? name[(lastDot + 1)..].ToLowerInvariant()
            : name.ToLowerInvariant();
    }

    /// <summary>Strip version suffix and convert PascalCase to kebab-case.</summary>
    private static string DeriveEventName(string typeName)
    {
        var cleaned = System.Text.RegularExpressions.Regex.Replace(typeName, @"V\d+$", "");
        return System.Text.RegularExpressions.Regex.Replace(cleaned,
            "([a-z])([A-Z])", "$1-$2").ToLowerInvariant();
    }

    private static Type FindEnvelopeType(Type eventType)
    {
        var openEnvelope = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t is { IsGenericTypeDefinition: true, Name: "MessageEnvelope`1" })
            ?? throw new InvalidOperationException(
                "Cannot find MessageEnvelope<> type. Ensure ECommerce.Services.Shared is referenced.");
        return openEnvelope.MakeGenericType(eventType);
    }

    /// <summary>Add Kafka message publisher (topic producer wrapper).</summary>
    public TransactionalMassTransitConfigurator<TDbContext> AddKafkaMessagePublisher<TMessage>()
        where TMessage : class
    {
        _configureKafkaPublisher += services =>
            services.AddScoped<IMassTransitMessagePublisher, KafkaTopicMessagePublisher<TMessage>>();
        return this;
    }

    internal IServiceCollection Apply()
    {
        ApplyTransport();

        _services.AddScoped<IMassTransitMessagePublisher, MassTransitEnvelopePublisher>();

        _services.AddScoped<IEventBus, MassTransitEventBus>();
        _services.AddScoped<IInternalCommandBus, MassTransitInternalCommandBus>();

        _configureServices?.Invoke(_services);
        return _services;
    }

    private void ApplyTransport()
    {
        switch (_transport)
        {
            case MessagingTransportType.RabbitMq:
                ApplyRabbitMq();
                break;
            case MessagingTransportType.Kafka:
                ApplyKafka();
                break;
            default:
                throw new InvalidOperationException($"Unsupported transport '{_transport}'.");
        }
    }

    private void ApplyRabbitMq()
    {
        if (_environment?.IsEnvironment("Test") == true)
        {
            _services.AddMassTransitTestHarness(cfg =>
            {
                if (_options.Bus.UseBusOutbox)
                    cfg.AddEntityFrameworkOutbox<TDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
                _configureBus?.Invoke(cfg);
                cfg.UsingRabbitMq((context, rabbit) =>
                {
                    rabbit.Host(new Uri(_options.RabbitMqConnectionString
                        ?? throw new InvalidOperationException("Missing connection string 'rabbitmq'.")));
                    _configureRabbitMq?.Invoke(context, rabbit);
                    rabbit.ConfigureEndpoints(context);
                });
            });
        }
        else
        {
            _services.AddMassTransit(cfg =>
            {
                if (_options.Bus.UseBusOutbox)
                    cfg.AddEntityFrameworkOutbox<TDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
                _configureBus?.Invoke(cfg);
                cfg.UsingRabbitMq((context, rabbit) =>
                {
                    rabbit.Host(new Uri(_options.RabbitMqConnectionString
                        ?? throw new InvalidOperationException("Missing connection string 'rabbitmq'.")));
                    _configureRabbitMq?.Invoke(context, rabbit);
                    rabbit.ConfigureEndpoints(context);
                });
            });
        }
    }

    private void ApplyKafka()
    {
        if (_environment?.IsEnvironment("Test") == true)
        {
            _services.AddMassTransitTestHarness(cfg =>
            {
                if (_options.Bus.UseBusOutbox)
                    cfg.AddEntityFrameworkOutbox<TDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
                _configureBus?.Invoke(cfg);
                cfg.AddRider(rider =>
                {
                    _configureKafkaRider?.Invoke(rider);
                    rider.UsingKafka((context, kafka) =>
                    {
                        kafka.Host(_options.KafkaConnectionString
                            ?? throw new InvalidOperationException("Missing connection string 'kafka'."));
                        _configureKafkaTransport?.Invoke(context, kafka);
                    });
                });
                cfg.UsingInMemory((context, mem) => mem.ConfigureEndpoints(context));
            });
        }
        else
        {
            _services.AddMassTransit(cfg =>
            {
                if (_options.Bus.UseBusOutbox)
                    cfg.AddEntityFrameworkOutbox<TDbContext>(o => { o.UsePostgres(); o.UseBusOutbox(); });
                _configureBus?.Invoke(cfg);
                cfg.AddRider(rider =>
                {
                    _configureKafkaRider?.Invoke(rider);
                    rider.UsingKafka((context, kafka) =>
                    {
                        kafka.Host(_options.KafkaConnectionString
                            ?? throw new InvalidOperationException("Missing connection string 'kafka'."));
                        _configureKafkaTransport?.Invoke(context, kafka);
                    });
                });
                cfg.UsingInMemory((context, mem) => mem.ConfigureEndpoints(context));
            });
        }

        _configureKafkaPublisher?.Invoke(_services);
    }
}
