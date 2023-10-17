using AutoMapper;
using Catalog.Data;
using Catalog.Products.Models;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.CreatingProduct;

public record CreateProductRequestDto(string Name, string Description, decimal Price);

public record CreateProductResponseDto(Guid Id);

public record CreateProduct(string Name, string Description, decimal Price) : IRequest<CreateProductResult>
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

public record CreateProductResult(Guid Id);

// Post api/catalog/products
public static class CreateProductEndpoint
{
    public static void MapCreateProductByIdEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost("api/catalog/products", async (CreateProductRequestDto request,
                IMediator mediator,
                IMapper mapper,
                CancellationToken cancellationToken) =>
            {
                var command = mapper.Map<CreateProduct>(request);

                var result = await mediator.Send(command, cancellationToken);

                var response = new CreateProductResponseDto(result.Id);

                return Results.Ok(response);
            })
            .Produces<CreateProductResponseDto>()
            .WithName("CreateProduct");
    }
}

internal class Handler : IRequestHandler<CreateProduct, CreateProductResult>
{
    private readonly CatalogDbContext _catalogDbContext;
    private readonly IMapper _mapper;

    public Handler(CatalogDbContext catalogDbContext, IMapper mapper)
    {
        _catalogDbContext = catalogDbContext;
        _mapper = mapper;
    }

    public async Task<CreateProductResult> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        var product = _mapper.Map<Product>(command);

        var entityEntry = (await _catalogDbContext.Products.AddAsync(product, cancellationToken)).Entity;
        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        return new CreateProductResult(entityEntry.Id);
    }
}