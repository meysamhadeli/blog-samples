using Tests.Shared.Factory;
using Xunit;

namespace Tests.Shared.Fixtures;

public abstract class SharedFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private readonly bool _useMongo;

    protected SharedFixture(bool useMongo = false)
    {
        _useMongo = useMongo;
        if (_useMongo) Mongo = new MongoContainerFixture();
    }

    public PostgresContainerFixture Postgres { get; } = new();
    public RabbitMqContainerFixture RabbitMq { get; } = new();
    public KafkaContainerFixture Kafka { get; } = new();
    public MongoContainerFixture? Mongo { get; }

    public virtual async ValueTask InitializeAsync()
    {
        await Postgres.InitializeAsync();
        await RabbitMq.InitializeAsync();
        await Kafka.InitializeAsync();
        if (Mongo is not null) await Mongo.InitializeAsync();
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (Mongo is not null) await Mongo.DisposeAsync();
        await Kafka.DisposeAsync();
        await RabbitMq.DisposeAsync();
        await Postgres.DisposeAsync();
    }

    public async Task ResetAsync(string transport, CancellationToken cancellationToken = default)
    {
        await Postgres.ResetAsync();
        if (Mongo is not null) await Mongo.ResetAsync(cancellationToken);

        if (string.Equals(transport, "kafka", StringComparison.OrdinalIgnoreCase))
        {
            await Kafka.EnsureStartedAsync();
            await Kafka.CleanupTopicsAsync(cancellationToken);
            return;
        }

        await RabbitMq.EnsureStartedAsync();
        await RabbitMq.CleanupQueuesAsync(cancellationToken);
    }

    public CustomWebApplicationFactory<TEntryPoint> CreateFactory(
        string transport,
        Action<CustomWebApplicationFactory<TEntryPoint>>? configure = null)
    {
        var factory = new CustomWebApplicationFactory<TEntryPoint>();

        // Always set DB connection strings (containers start eagerly)
        factory.WithSetting("ConnectionStrings:catalogsdb", Postgres.ConnectionString);
        factory.WithSetting("ConnectionStrings:ordersdb", Postgres.ConnectionString);
        if (Mongo is not null)
            factory.WithSetting("ConnectionStrings:catalogs-mongo", Mongo.ConnectionString);
        factory.WithSetting("Messaging:Transport", transport);

        // Only set the active broker connection string — accessing Kafka.BootstrapServers
        // when container never started throws "Could not find resource 'KafkaContainer'"
        if (string.Equals(transport, "kafka", StringComparison.OrdinalIgnoreCase))
            factory.WithSetting("ConnectionStrings:kafka", Kafka.BootstrapServers);
        else
            factory.WithSetting("ConnectionStrings:rabbitmq", RabbitMq.ConnectionString);

        ConfigureFactory(factory, transport);
        configure?.Invoke(factory);
        return factory;
    }

    /// <summary>
    /// Optional per-test-project factory customisation.
    /// Called after default connection strings are set.
    /// </summary>
    protected virtual void ConfigureFactory(
        CustomWebApplicationFactory<TEntryPoint> factory, string transport) { }
}
