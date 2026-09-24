using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Order.Data;
using Order.Orders.Models;

namespace Order.Orders.Features.CreatingOrder;

public record CreateOrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record CreateOrderRequestDto(
    string CustomerName,
    string ShippingAddress,
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemResult(Guid Id, Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record CreateOrderResult(Guid Id, string CustomerName, decimal TotalPrice, string Status, List<CreateOrderItemResult> Items);

public record CreateOrderItem(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record CreateOrder(
    string CustomerName,
    string ShippingAddress,
    List<CreateOrderItem> Items
) : IRequest<CreateOrderResult>
{
    public Guid Id { get; init; } = Guid.NewGuid();
}

// POST api/orders
public static class CreateOrderEndpoint
{
    public static void MapCreateOrderEndpoint(this IEndpointRouteBuilder endpoint)
    {
        endpoint
            .MapPost("api/orders", async (
                CreateOrderRequestDto request,
                IMediator mediator,
                IMapper mapper,
                CancellationToken cancellationToken) =>
            {
                var command = mapper.Map<CreateOrder>(request);

                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(result);
            })
            .WithName("CreateOrder");
    }
}

internal class Handler : IRequestHandler<CreateOrder, CreateOrderResult>
{
    private readonly OrderDbContext _orderDbContext;
    private readonly IMapper _mapper;

    public Handler(OrderDbContext orderDbContext, IMapper mapper)
    {
        _orderDbContext = orderDbContext;
        _mapper = mapper;
    }

    public async Task<CreateOrderResult> Handle(
        CreateOrder command,
        CancellationToken cancellationToken)
    {
        var order = new Orders.Models.Order
        {
            Id = command.Id,
            CustomerName = command.CustomerName,
            ShippingAddress = command.ShippingAddress,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            Items = command.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                OrderId = command.Id
            }).ToList(),
            TotalPrice = command.Items.Sum(i => i.Quantity * i.UnitPrice)
        };

        var entityEntry = (await _orderDbContext.Orders
            .AddAsync(order, cancellationToken)).Entity;
        await _orderDbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CreateOrderResult>(entityEntry);
    }
}
