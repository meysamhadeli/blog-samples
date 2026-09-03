using Tests.Shared.TestBase;
using Xunit;

namespace Order.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<OrdersSharedFixture>
{
    public const string Name = Tests.Shared.TestBase.IntegrationTestCollection.Name;
}
