using Catalog.Data;
using Catalog.Products.Features.CreatingProduct;
using Catalog.Products.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Catalog.Extensions.Infrastructure;

public static class InfrastructureExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<CatalogDbContext>(opt =>
            opt.UseInMemoryDatabase(nameof(Product))
        );
        builder.Services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ProductRoot).Assembly)
        );

        builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ProductRoot).Assembly));

        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapCreateProductByIdEndpoint();

        app.UseHttpsRedirection();
        return app;
    }
}
