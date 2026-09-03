using BuildingBlocks.Integration.Wolverine.Abstractions;
using Contracts.Messages.MessageEnvelope;
using Contracts.Messages.ProductCreated;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Order.Products.Features.ConsumingProductCreated.v1;

internal static class ReceiveProductCreatedEndpoint
{
    internal static RouteHandlerBuilder MapReceiveProductCreatedEndpoint(this IEndpointRouteBuilder endpoints)
        => endpoints.MapPost("/products/receive", Handle).WithName("ReceiveProductCreated");

    private static async Task<IResult> Handle(
        [FromBody] MessageEnvelope<ProductCreatedV1> envelope,
        IEventBus eventBus,
        CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(envelope, cancellationToken);
        return Results.Accepted();
    }
}
