using AutoMapper;
using Catalog.Data;
using Catalog.Products.Models;
using MediatR;

namespace Catalog.Products.Features.CreatingProduct;

public record CreateProduct(string Name, string Description, decimal Price) : IRequest<CreateProductResult>
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

public record CreateProductResult(Guid Id, string Name, string Description, decimal Price);

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

        var entityEntry = await _catalogDbContext.Products.AddAsync(product, cancellationToken);
        await _catalogDbContext.SaveChangesAsync(cancellationToken);

        var createProductResult = _mapper.Map<CreateProductResult>(entityEntry.Entity);

        return createProductResult;
    }
}