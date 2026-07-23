using Contracts.Messages.MessageEnvelope;
using Contracts.Messages.ProductCreated;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Products.Models;
using Order.Shared.Data;

namespace Order.Products.Features.ConsumingProductCreated.v1;

public record ProductCreatedNotification(MessageEnvelope<ProductCreatedV1> Envelope) : INotification;

public sealed class ProductCreatedNotificationHandler(OrdersDbContext dbContext)
    : INotificationHandler<ProductCreatedNotification>
{
    public async Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        var message = notification.Envelope.Message;
        if (string.Equals(message.Name, "faulty-product-created", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Intentional consumer failure for retry and dead-letter tests.");

        var existing = await dbContext.ImportedProducts.SingleOrDefaultAsync(
            x => x.Id == message.Id, cancellationToken);

        if (existing is null)
            dbContext.ImportedProducts.Add(ImportedProduct.Create(
                message.Id, message.Name, message.Price, 0, message.CreatedAtUtc));
        else
            existing.Update(message.Name, message.Price, 0, message.CreatedAtUtc);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
