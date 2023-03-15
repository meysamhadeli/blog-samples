using Catalog;
using Catalog.Data;
using Catalog.Products.Features.CreatingProduct;
using Catalog.Products.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CatalogDbContext>(opt => opt.UseInMemoryDatabase(nameof(Product)));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductRoot).Assembly));
builder.Services.AddAutoMapper(typeof(ProductRoot).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCreateProductByIdEndpoint();

app.UseHttpsRedirection();
app.Run();

