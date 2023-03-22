using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Products.Features.CreatingProduct;

// Post api/catalog/products
public static class CreateProductEndpoint
{
    public record CreateProductRequestDto(string Name, string Description, decimal Price);
    public record CreateProductResponseDto(Guid Id);

    public static void MapCreateProductByIdEndpoint(this IEndpointRouteBuilder endpoint)
    {
         endpoint
            .MapPost("api/catalog/products", CreateProduct)
            .Produces<CreateProductResponseDto>()
            .WithName("CreateProduct");
    }
    
    private static async Task<IResult> CreateProduct(
        CreateProductRequestDto request,
        IMediator mediator,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var command = mapper.Map<CreateProduct>(request);

        var result = await mediator.Send(command, cancellationToken);

        var response = new CreateProductResponseDto(result.Id);
        
        return Results.Ok(response);
    }
}