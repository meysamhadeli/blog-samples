using Tests.Shared.Factory;

namespace Tests.Shared.Fixtures;

public abstract class SharedFixture<TEntryPoint> : IAsyncLifetime
    where TEntryPoint : class
{
    private string? _deepSeekApiKey;

    public CustomWebApplicationFactory<TEntryPoint> Factory { get; } = new();

    public virtual ValueTask InitializeAsync()
    {
        _deepSeekApiKey = Environment.GetEnvironmentVariable("DS_KEY")
            ?? Environment.GetEnvironmentVariable("DS_KEY", EnvironmentVariableTarget.User);

        if (!string.IsNullOrWhiteSpace(_deepSeekApiKey))
            Environment.SetEnvironmentVariable("DS_KEY", _deepSeekApiKey);

        return ValueTask.CompletedTask;
    }

    public virtual ValueTask DisposeAsync() => Factory.DisposeAsync();

    protected void RequireDeepSeekApiKey()
    {
        if (string.IsNullOrWhiteSpace(_deepSeekApiKey))
            Assert.Skip("DS_KEY is required for the live DeepSeek integration test.");
    }
}
