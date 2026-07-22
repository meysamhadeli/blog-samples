namespace BuildingBlocks.Integration.MassTransit;

public sealed class DurableCommandProcessorOptions
{
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(2);
    public int BatchSize { get; set; } = 10;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan StaleProcessingThreshold { get; set; } = TimeSpan.FromMinutes(5);
}
