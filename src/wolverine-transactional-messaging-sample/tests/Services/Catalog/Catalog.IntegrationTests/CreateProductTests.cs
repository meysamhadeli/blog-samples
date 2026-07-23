using System.Net.Http.Json;
using Catalog.Products.Features.CreatingProduct.v1;
using Catalog.Products.Models;
using Catalog.Shared.Data;
using Catalog.Shared.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Catalog.IntegrationTests;

public class CreateProductTests : CatalogsIntegrationTestBase
{
    public CreateProductTests(CatalogsSharedFixture sharedFixture) : base(sharedFixture) { }

    [Fact]
    public async Task CreateProduct_ShouldPersistInPostgres()
    {
        var request = new CreateProductRequest("Test Product", 29.99m, 10);

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/catalogs/products", request);
        response.EnsureSuccessStatusCode();

        await ExecuteCatalogsDbContextAsync(async dbContext =>
        {
            var product = await dbContext.Products.SingleAsync(x => x.Name == "Test Product");
            Assert.Equal(29.99m, product.Price);
            Assert.Equal(10, product.Stock);
        });
    }

    [Fact]
    public async Task CreateProduct_ShouldProjectToMongoReadModel()
    {
        var request = new CreateProductRequest("Mongo Product", 15.50m, 5);

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/catalogs/products", request);
        response.EnsureSuccessStatusCode();

        await Task.Delay(500);

        using var scope = Factory.Services.CreateAsyncScope();
        var mongoDatabase = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var collection = mongoDatabase.GetCollection<ProductReadModel>("product-read-models");
        var readModel = await collection.Find(x => x.Name == "Mongo Product").FirstOrDefaultAsync();

        Assert.NotNull(readModel);
        Assert.Equal(15.50m, readModel.Price);
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnAcceptedWithProductId()
    {
        var request = new CreateProductRequest("New Item", 99.99m, 50);

        var response = await Factory.CreateClient().PostAsJsonAsync("/api/v1/catalogs/products", request);
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<CreateProductResponse>();
        Assert.NotNull(result);
        Assert.Equal("New Item", result.Name);
    }
}
