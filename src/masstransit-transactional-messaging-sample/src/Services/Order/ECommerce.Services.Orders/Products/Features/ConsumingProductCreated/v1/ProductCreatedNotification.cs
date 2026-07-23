using ECommerce.Services.Orders.Products.Models;
using ECommerce.Services.Orders.Shared.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services.Orders.Products.Features.ConsumingProductCreated.v1;

public sealed record ProductCreatedNotification(
    Guid ProductId, string Name, decimal Price, int Stock, DateTime CreatedAtUtc
) : INotification;

public sealed class ProductCreatedNotificationHandler(OrdersDbContext dbContext)
    : INotificationHandler<ProductCreatedNotification>
{
    public async Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        if (string.Equals(notification.Name, "faulty-product-created", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Intentional consumer failure for retry and dead-letter tests.");

        var existing = await dbContext.ImportedProducts.SingleOrDefaultAsync(
            x => x.Id == notification.ProductId, cancellationToken);

        if (existing is null)
            dbContext.ImportedProducts.Add(ImportedProduct.Create(
                notification.ProductId, notification.Name, notification.Price, notification.Stock, notification.CreatedAtUtc));
        else
            existing.Update(notification.Name, notification.Price, notification.Stock, notification.CreatedAtUtc);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
