using AutoMapper;
using Order.Orders.Features.CreatingOrder;
using Order.Orders.Features.GettingOrderById;
using Order.Orders.Features.ListingOrders;
using Order.Orders.Models;

namespace Order.Orders.Features;

public class OrderMappings : Profile
{
    public OrderMappings()
    {
        CreateMap<CreateOrderRequestDto, CreateOrder>();
        CreateMap<CreateOrderItemDto, CreateOrderItem>();
        CreateMap<CreateOrder, Orders.Models.Order>();
        CreateMap<OrderItem, CreateOrderItemResult>();
        CreateMap<Orders.Models.Order, CreateOrderResult>();
        CreateMap<Orders.Models.Order, GetOrderByIdResult>();
        CreateMap<OrderItem, GetOrderItemResult>();
        CreateMap<Orders.Models.Order, GetOrdersResult>();
    }
}
