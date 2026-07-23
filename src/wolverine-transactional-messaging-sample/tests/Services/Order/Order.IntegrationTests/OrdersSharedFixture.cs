using Tests.Shared.Factory;
using Tests.Shared.Fixtures;

namespace Order.IntegrationTests;

public sealed class OrdersSharedFixture : SharedFixture<Program>
{
    public OrdersSharedFixture() : base(useMongo: false) { }

    protected override void ConfigureFactory(
        CustomWebApplicationFactory<Program> factory, string transport)
    {
        // No Order-specific overrides needed — base sets all standard connection strings.
    }
}
