using Xunit;
using Catalog.Models;
using Tests.Shared;

namespace Catalog.Tests;

public sealed class CatalogIntegrationTests
{
    public static TheoryData<string> SupportedTransports => new()
    {
        "rabbitmq",
        "kafka"
    };

    [Theory]
    [MemberData(nameof(SupportedTransports))]
    public async Task create_product_should_persist_write_model_and_project_read_model_for_supported_brokers(string transport)
    {
        var flow = new SampleFlow();

        var result = await flow.RunAsync(transport);

        Assert.Equal(transport, result.Transport);
        Assert.True(result.Imported);
        Assert.NotNull(result.WriteModel);
        Assert.NotNull(result.ReadModel);
        Assert.NotNull(result.OrderProduct);
        Assert.IsType<Product>(result.WriteModel);
        Assert.IsType<ProductReadModel>(result.ReadModel);
    }
}
