using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Order.Data;

namespace Order.Orders.Features.ListingOrders;

public record GetOrdersResult(
    Guid Id, string CustomerName, decimal TotalPrice,
    string Status, DateTime CreatedAt
);

public record GetOrdersQuery() : IRequest<List<GetOrdersResult>>;

// GET api/orders
public static class GetOrdersEndpoint
{
    public static void MapGetOrdersEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet("api/orders", async (
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetOrdersQuery();
                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("GetOrders");
    }
}

internal class Handler : IRequestHandler<GetOrdersQuery, List<GetOrdersResult>>
{
    private readonly OrderDbContext _orderDbContext;
    private readonly IMapper _mapper;

    public Handler(OrderDbContext orderDbContext, IMapper mapper)
    {
        _orderDbContext = orderDbContext;
        _mapper = mapper;
    }

    public async Task<List<GetOrdersResult>> Handle(
        GetOrdersQuery query,
        CancellationToken cancellationToken)
    {
        var orders = await _orderDbContext.Orders
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<GetOrdersResult>>(orders);
    }
}
