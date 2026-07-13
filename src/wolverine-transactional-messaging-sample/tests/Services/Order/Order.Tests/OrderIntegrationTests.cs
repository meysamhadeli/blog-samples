using Xunit;
using Contracts;
using Order;
using Order.Data;

namespace Order.Tests;

public sealed class OrderIntegrationTests
{
    [Fact]
    public async Task import_should_be_idempotent_for_duplicate_messages()
    {
        var service = new OrderImportService(new OrderImportStore(), new InboxStore());
        var message = MessageEnvelope<ProductCreatedV1>.Create(new ProductCreatedV1(Guid.NewGuid(), "Mouse", 49.99m, 20));

        var first = await service.ImportAsync(message);
        var second = await service.ImportAsync(message);

        Assert.True(first);
        Assert.False(second);
    }
}
