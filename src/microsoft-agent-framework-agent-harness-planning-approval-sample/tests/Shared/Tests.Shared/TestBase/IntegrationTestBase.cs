using System.Net.Http;
using Tests.Shared.Fixtures;

namespace Tests.Shared.TestBase;

public abstract class IntegrationTestBase<TEntryPoint, TFixture> : IAsyncLifetime
    where TEntryPoint : class
    where TFixture : SharedFixture<TEntryPoint>
{
    protected IntegrationTestBase(TFixture sharedFixture)
    {
        Client = sharedFixture.Factory.CreateClient();
    }

    protected HttpClient Client { get; }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
    }
}