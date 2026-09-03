using Tests.Shared.Factory;
namespace Tests.Shared.Fixtures;
public abstract class SharedFixture<TEntryPoint> : IAsyncLifetime where TEntryPoint : class
{
    public CustomWebApplicationFactory<TEntryPoint> Factory { get; } = new();
    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public ValueTask DisposeAsync() => Factory.DisposeAsync();
}