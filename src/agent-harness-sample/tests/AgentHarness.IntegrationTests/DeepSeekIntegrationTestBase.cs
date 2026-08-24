using Tests.Shared.TestBase;

namespace AgentHarness.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public abstract class DeepSeekIntegrationTestBase
    : IntegrationTestBase<Program, AgentHarnessSharedFixture>
{
    protected DeepSeekIntegrationTestBase(AgentHarnessSharedFixture sharedFixture)
        : base(sharedFixture)
    {
        sharedFixture.RequireDeepSeekApiKey();
    }
}