namespace Tests.Shared.TestBase;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<object>
{
    public const string Name = "Integration tests";
}
