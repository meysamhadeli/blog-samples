namespace AgentHarness.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<AgentHarnessSharedFixture>
{
    public const string Name = "Integration tests";
}