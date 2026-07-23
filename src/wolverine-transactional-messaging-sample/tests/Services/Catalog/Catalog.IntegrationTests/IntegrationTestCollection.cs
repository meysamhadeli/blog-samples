using Tests.Shared.TestBase;
using Xunit;

namespace Catalog.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<CatalogsSharedFixture>
{
    public const string Name = Tests.Shared.TestBase.IntegrationTestCollection.Name;
}
