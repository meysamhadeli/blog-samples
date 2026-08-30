using backend.Products.Models;
using MediatR;

namespace backend.Products.Features.GettingProducts.v1;

public sealed record GetProductsQuery : IRequest<IReadOnlyList<Product>>;

internal sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, IReadOnlyList<Product>>
{
    public Task<IReadOnlyList<Product>> Handle(
        GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products =
        [
            new(1, "Aurora Headphones", "Audio", 129.00m, 18, "Wireless noise-cancelling headphones with thirty-hour battery life."),
            new(2, "Field Notes Pack", "Stationery", 18.50m, 42, "Three pocket notebooks made for daily ideas and durable enough for travel."),
            new(3, "Moss Desk Lamp", "Workspace", 76.00m, 9, "Adjustable warm light with a compact recycled-aluminium base."),
            new(4, "Canvas Weekender", "Travel", 148.00m, 6, "Structured cotton canvas bag with leather handles and a shoe compartment.")
        ];

        return Task.FromResult(products);
    }
}
