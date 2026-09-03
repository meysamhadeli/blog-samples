using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Testcontainers.Kafka;
using Xunit;

namespace Tests.Shared.Fixtures;

public sealed class KafkaContainerFixture : IAsyncLifetime
{
    private const string LocalKafkaImage = "confluentinc/cp-kafka:7.5.12";
    private bool _started;

    public KafkaContainer Container { get; } =
        new KafkaBuilder().WithImage(LocalKafkaImage).WithKRaft().Build();

    public string BootstrapServers => Container.GetBootstrapAddress();

    public async Task EnsureStartedAsync()
    {
        if (_started) return;
        await Container.StartAsync();
        _started = true;
    }

    public async Task CleanupTopicsAsync(CancellationToken cancellationToken = default)
    {
        if (!_started) return;

        var client = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = BootstrapServers }
        ).Build();

        var topics = client
            .GetMetadata(TimeSpan.FromSeconds(5))
            .Topics.Where(x => !x.Topic.StartsWith("__", StringComparison.Ordinal))
            .Select(x => x.Topic)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (topics.Length == 0) return;

        try { await client.DeleteTopicsAsync(topics); }
        catch { /* best effort cleanup */ }
    }

    public async Task EnsureTopicsAsync(IEnumerable<string> topicNames, CancellationToken cancellationToken = default)
    {
        if (!_started) await EnsureStartedAsync();

        var requestedTopics = topicNames
            .Where(topicName => !string.IsNullOrWhiteSpace(topicName))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedTopics.Length == 0) return;

        var client = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = BootstrapServers }
        ).Build();

        try
        {
            await client.CreateTopicsAsync(
                requestedTopics.Select(name => new TopicSpecification
                {
                    Name = name,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }));
        }
        catch (CreateTopicsException) { /* topic may already exist */ }
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_started) await Container.DisposeAsync();
    }
}
