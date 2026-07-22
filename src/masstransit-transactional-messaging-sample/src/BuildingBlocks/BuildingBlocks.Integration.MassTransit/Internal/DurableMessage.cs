namespace BuildingBlocks.Integration.MassTransit;

public sealed class DurableMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string TypeName { get; init; } = default!;
    public string Payload { get; init; } = default!;
    public DurableMessageStatus Status { get; set; } = DurableMessageStatus.Pending;
    public DateTime EnqueuedAtUtc { get; init; } = DateTime.UtcNow;
    public DateTime? LastAttemptAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public enum DurableMessageStatus { Pending = 0, Processing = 1, Completed = 2, Failed = 3 }
