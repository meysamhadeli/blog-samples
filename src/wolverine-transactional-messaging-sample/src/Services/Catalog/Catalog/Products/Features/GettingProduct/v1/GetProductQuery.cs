using Catalog.Products.Models;
using Catalog.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Products.Features.GettingProduct.v1;

public sealed record GetProductQuery(Guid Id) : IRequest<Product?>;

internal sealed class GetProductHandler(CatalogsDbContext dbContext)
    : IRequestHandler<GetProductQuery, Product?>
{
    public async Task<Product?> Handle(GetProductQuery query, CancellationToken cancellationToken)
        => await dbContext.Products.SingleOrDefaultAsync(
            x => x.Id == query.Id, cancellationToken);
}
