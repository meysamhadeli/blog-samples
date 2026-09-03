using System.Net.Http.Json;
using Contracts.Messages.MessageEnvelope;
using Contracts.Messages.ProductCreated;
using Microsoft.EntityFrameworkCore;
using Order.Shared.Data;

namespace Order.IntegrationTests;

public class ProductCreatedEndpointTests : OrdersIntegrationTestBase
{
    public ProductCreatedEndpointTests(OrdersSharedFixture sharedFixture) : base(sharedFixture) { }

    [Fact]
    public async Task ConsumeProductCreated_ShouldInsert_ImportedProduct_WhenMissing()
    {
        var envelope = MessageEnvelope.Create(new ProductCreatedV1(
            Guid.NewGuid(),
            "New Product",
            "Category",
            "Description",
            19.99m,
            DateTime.UtcNow
        ));

        var response = await Factory.CreateClient().PostAsJsonAsync(
            "/api/v1/orders/products/receive", envelope);
        response.EnsureSuccessStatusCode();

        await Task.Delay(500);

        await ExecuteOrdersDbContextAsync(async dbContext =>
        {
            var imported = await dbContext.ImportedProducts.SingleAsync(x => x.Id == envelope.Message.Id);
            Assert.Equal("New Product", imported.Name);
            Assert.Equal(19.99m, imported.Price);
        });
    }

    [Fact]
    public async Task ConsumeProductCreated_ShouldUpdate_ImportedProduct_WhenAlreadyExists()
    {
        var productId = Guid.NewGuid();
        var originalEnvelope = MessageEnvelope.Create(new ProductCreatedV1(
            productId,
            "Original Name",
            "Category",
            "Description",
            10.00m,
            DateTime.UtcNow.AddDays(-1)
        ));

        var createResponse = await Factory.CreateClient().PostAsJsonAsync(
            "/api/v1/orders/products/receive", originalEnvelope);
        createResponse.EnsureSuccessStatusCode();
        await Task.Delay(500);

        var updateEnvelope = MessageEnvelope.Create(new ProductCreatedV1(
            productId,
            "Updated Name",
            "Category",
            "Description",
            29.99m,
            DateTime.UtcNow
        ));

        var updateResponse = await Factory.CreateClient().PostAsJsonAsync(
            "/api/v1/orders/products/receive", updateEnvelope);
        updateResponse.EnsureSuccessStatusCode();
        await Task.Delay(500);

        await ExecuteOrdersDbContextAsync(async dbContext =>
        {
            var imported = await dbContext.ImportedProducts.SingleAsync(x => x.Id == productId);
            Assert.Equal("Updated Name", imported.Name);
            Assert.Equal(29.99m, imported.Price);
        });
    }

    [Theory]
    [InlineData("faulty-product-created")]
    public async Task ConsumeProductCreated_ShouldThrow_ForFaultyProductName(string faultyName)
    {
        var productId = Guid.NewGuid();
        var envelope = MessageEnvelope.Create(new ProductCreatedV1(
            productId,
            faultyName,
            "Category",
            "Description",
            5.00m,
            DateTime.UtcNow
        ));

        var response = await Factory.CreateClient().PostAsJsonAsync(
            "/api/v1/orders/products/receive", envelope);

        // Endpoint returns 202 Accepted immediately (async messaging pattern)
        Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);

        // Wait for async processing to fail
        await Task.Delay(1000);

        // Verify product was NOT inserted (handler threw before writing)
        await ExecuteOrdersDbContextAsync(async dbContext =>
        {
            var exists = await dbContext.ImportedProducts.AnyAsync(x => x.Id == productId);
            Assert.False(exists);
        });
    }
}
