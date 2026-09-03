using AutoMapper;
using Catalog.Data;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.GettingProductById;

public record GetProductByIdRequestDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
);

public record GetProductByIdResult(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Category
);

public record GetProductByIdQuery(Guid Id) : IRequest<GetProductByIdResult>;

// GET api/catalog/products/{id}
public static class GetProductByIdEndpoint
{
    public static void MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet("api/catalog/products/{id}", async (
                Guid id,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetProductByIdQuery(id);
                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("GetProductById");
    }
}

internal class Handler : IRequestHandler<GetProductByIdQuery, GetProductByIdResult>
{
    private readonly CatalogDbContext _catalogDbContext;
    private readonly IMapper _mapper;

    public Handler(CatalogDbContext catalogDbContext, IMapper mapper)
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
    }

    public async Task<GetProductByIdResult> Handle(
        GetProductByIdQuery query,
        CancellationToken cancellationToken)
    {
        var product = await _catalogDbContext.Products
            .FindAsync(new object[] { query.Id }, cancellationToken);

        if (product is null)
            throw new KeyNotFoundException($"Product with id {query.Id} not found.");

        return _mapper.Map<GetProductByIdResult>(product);
    }
}
