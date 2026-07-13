using Xunit;
using Catalog.Models;
using Tests.Shared;

namespace Catalog.Tests;

public sealed class CatalogIntegrationTests
{
    [Fact]
    public async Task create_product_should_persist_write_model_and_project_read_model()
    {
        var flow = new SampleFlow();

        var result = await flow.RunAsync();

        Assert.True(result.Imported);
        Assert.NotNull(result.WriteModel);
        Assert.NotNull(result.ReadModel);
        Assert.IsType<Product>(result.WriteModel);
        Assert.IsType<ProductReadModel>(result.ReadModel);
    }
}
