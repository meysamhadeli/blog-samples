using Tests.Shared.Factory;
using Tests.Shared.Fixtures;

namespace Tests.Shared.TestBase;

public abstract class IntegrationTestBase<TEntryPoint, TSharedFixture>
    : IClassFixture<TSharedFixture>
    where TEntryPoint : class
    where TSharedFixture : SharedFixture<TEntryPoint>
{
    protected IntegrationTestBase(TSharedFixture sharedFixture)
    {
        SharedFixture = sharedFixture;
        Client = sharedFixture.Factory.CreateClient();
    }

    protected TSharedFixture SharedFixture { get; }
    protected HttpClient Client { get; }
}