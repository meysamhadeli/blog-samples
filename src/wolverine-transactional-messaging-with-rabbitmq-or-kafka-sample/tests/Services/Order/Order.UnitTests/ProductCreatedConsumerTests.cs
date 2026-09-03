using Microsoft.EntityFrameworkCore;
using Order.Products.Features.ConsumingProductCreated.v1;
using Order.Products.Models;
using Order.Shared.Data;
using Tests.Shared;

namespace Order.UnitTests;

public class ProductCreatedConsumerTests
{
    [Fact]
    public async Task Consume_ShouldInsert_ImportedProduct_WhenMissing()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ProductCreatedNotificationHandler(dbContext);
        var envelope = SampleData.ProductCreatedEnvelope();

        await handler.Handle(new ProductCreatedNotification(envelope), default);

        var imported = await dbContext.ImportedProducts.SingleAsync();

        Assert.Equal(SampleData.ProductId, imported.Id);
        Assert.Equal("Starter Basket", imported.Name);
        Assert.Equal(42.50m, imported.Price);
    }

    [Fact]
    public async Task Consume_ShouldUpdate_ImportedProduct_WhenAlreadyExists()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ImportedProducts.Add(
            ImportedProduct.Create(
                SampleData.ProductId,
                "Old Name",
                10m,
                5,
                SampleData.CreatedAtUtc.AddDays(-1)
            )
        );
        await dbContext.SaveChangesAsync();

        var handler = new ProductCreatedNotificationHandler(dbContext);
        var envelope = SampleData.ProductCreatedEnvelope();

        await handler.Handle(new ProductCreatedNotification(envelope), default);

        var imported = await dbContext.ImportedProducts.SingleAsync();

        Assert.Equal("Starter Basket", imported.Name);
        Assert.Equal(42.50m, imported.Price);
        Assert.Equal(SampleData.CreatedAtUtc, imported.CreatedAtUtc);
    }

    private static OrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new OrdersDbContext(options);
    }
}
