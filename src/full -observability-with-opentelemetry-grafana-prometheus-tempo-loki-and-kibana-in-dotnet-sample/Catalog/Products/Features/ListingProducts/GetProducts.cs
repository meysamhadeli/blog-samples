using AutoMapper;
using Catalog.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Products.Features.ListingProducts;

public record GetProductsResult(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
);

public record GetProductsQuery() : IRequest<List<GetProductsResult>>;

// GET api/catalog/products
public static class GetProductsEndpoint
{
    public static void MapGetProductsEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet("api/catalog/products", async (
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetProductsQuery();
                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("GetProducts");
    }
}

internal class Handler : IRequestHandler<GetProductsQuery, List<GetProductsResult>>
{
    private readonly CatalogDbContext _catalogDbContext;
    private readonly IMapper _mapper;

    public Handler(CatalogDbContext catalogDbContext, IMapper mapper)
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
    }

    public async Task<List<GetProductsResult>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        var products = await _catalogDbContext.Products
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<GetProductsResult>>(products);
    }
}
