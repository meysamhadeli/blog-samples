using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Catalogs.Products.Features.GettingProduct.v1;

internal sealed record GetProductQuery(Guid Id) : IRequest<Product?>;

internal sealed class GetProductHandler(CatalogsDbContext dbContext)
    : IRequestHandler<GetProductQuery, Product?>
{
    public async Task<Product?> Handle(GetProductQuery query, CancellationToken cancellationToken)
        => await dbContext.Products.SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
}
