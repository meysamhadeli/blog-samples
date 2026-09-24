using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Order.Data;

namespace Order.Orders.Features.GettingOrderById;

public record GetOrderItemResult(
    Guid Id, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice
);

public record GetOrderByIdResult(
    Guid Id, string CustomerName, string ShippingAddress,
    decimal TotalPrice, string Status, DateTime CreatedAt,
    List<GetOrderItemResult> Items
);

public record GetOrderByIdQuery(Guid Id) : IRequest<GetOrderByIdResult>;

// GET api/orders/{id}
public static class GetOrderByIdEndpoint
{
    public static void MapGetOrderByIdEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapGet("api/orders/{id}", async (
                Guid id,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var query = new GetOrderByIdQuery(id);
                var result = await mediator.Send(query, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("GetOrderById");
    }
}

internal class Handler : IRequestHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    private readonly OrderDbContext _orderDbContext;
    private readonly IMapper _mapper;

    public Handler(OrderDbContext orderDbContext, IMapper mapper)
    {
        _orderDbContext = orderDbContext;
        _mapper = mapper;
    }

    public async Task<GetOrderByIdResult> Handle(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken)
    {
        var order = await _orderDbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == query.Id, cancellationToken);

        if (order is null)
            throw new KeyNotFoundException($"Order with id {query.Id} not found.");

        return _mapper.Map<GetOrderByIdResult>(order);
    }
}
