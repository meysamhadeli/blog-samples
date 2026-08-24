using Tests.Shared.TestBase;

namespace AgentHarness.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<AgentHarnessSharedFixture>
{
    public const string Name = Tests.Shared.TestBase.IntegrationTestCollection.Name;
}