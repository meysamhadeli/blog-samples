using Tests.Shared.Factory;
using Tests.Shared.Fixtures;

namespace Catalog.IntegrationTests;

public sealed class CatalogsSharedFixture : SharedFixture<Program>
{
    public CatalogsSharedFixture() : base(useMongo: true) { }

    public string MongoConnectionString =>
        Mongo?.ConnectionString
        ?? throw new InvalidOperationException("MongoDB fixture not configured.");

    protected override void ConfigureFactory(
        CustomWebApplicationFactory<Program> factory, string transport)
    {
        // No Catalog-specific overrides needed — base sets all standard connection strings.
    }
}
