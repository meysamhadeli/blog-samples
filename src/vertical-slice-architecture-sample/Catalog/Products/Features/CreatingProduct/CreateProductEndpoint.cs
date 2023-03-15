using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.CreatingProduct;

public static class GetProductByIdEndpoint
{
    public record CreateProductRequestDto(string Name, string Description, decimal Price);

    public static void MapGetProductByIdEndpoint(this IEndpointRouteBuilder endpoint)
    {
         endpoint
            .MapPost("api/catalog/product", CreateProduct)
            .Produces<CreateProduct.CreateProductResult>()
            .WithName("CreateProduct");
    }
    
    private static async Task<IResult> CreateProduct(
        CreateProductRequestDto request,
        IMediator mediator,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var command = mapper.Map<CreateProduct.CreateProductCommand>(request);

        var queryResult = await mediator.Send(command, cancellationToken);
        
        return Results.Ok(queryResult);
    }
}