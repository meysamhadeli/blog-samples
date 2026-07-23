using ECommerce.Services.Orders.Products.Features.ConsumingProductCreated.v1;
using ECommerce.Services.Orders.Products.Models;
using ECommerce.Services.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Orders.UnitTests;

public class ProductCreatedNotificationHandlerTests
{
    [Fact]
    public async Task Handle_ShouldInsert_ImportedProduct_WhenMissing()
    {
        await using var dbContext = CreateDbContext();
        var handler = new ProductCreatedNotificationHandler(dbContext);
        var notification = new ProductCreatedNotification(
            Tests.Shared.SampleData.ProductId,
            "Starter Basket",
            42.50m,
            25,
            Tests.Shared.SampleData.CreatedAtUtc
        );

        await handler.Handle(notification, CancellationToken.None);

        var imported = await dbContext.ImportedProducts.SingleAsync();

        Assert.Equal(Tests.Shared.SampleData.ProductId, imported.Id);
        Assert.Equal(25, imported.Stock);
        Assert.Equal("Starter Basket", imported.Name);
        Assert.Equal(42.50m, imported.Price);
    }

    [Fact]
    public async Task Handle_ShouldUpdate_ImportedProduct_WhenAlreadyExists()
    {
        await using var dbContext = CreateDbContext();
        dbContext.ImportedProducts.Add(
            ImportedProduct.Create(
                Tests.Shared.SampleData.ProductId,
                "Old Name",
                10m,
                5,
                Tests.Shared.SampleData.CreatedAtUtc.AddDays(-1)
            )
        );
        await dbContext.SaveChangesAsync();

        var handler = new ProductCreatedNotificationHandler(dbContext);
        var notification = new ProductCreatedNotification(
            Tests.Shared.SampleData.ProductId,
            "Starter Basket",
            42.50m,
            25,
            Tests.Shared.SampleData.CreatedAtUtc
        );

        await handler.Handle(notification, CancellationToken.None);

        var imported = await dbContext.ImportedProducts.SingleAsync();

        Assert.Equal(25, imported.Stock);
        Assert.Equal("Starter Basket", imported.Name);
        Assert.Equal(42.50m, imported.Price);
        Assert.Equal(Tests.Shared.SampleData.CreatedAtUtc, imported.CreatedAtUtc);
    }

    private static OrdersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new OrdersDbContext(options);
    }
}
