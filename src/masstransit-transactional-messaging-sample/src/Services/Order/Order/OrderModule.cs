using Contracts;
using MassTransit;
using Order.Data;
using Order.Models;

namespace Order;

public sealed class OrderImportService : IConsumer<MessageEnvelope<ProductCreatedV1>>
{
    private readonly OrderImportStore _importStore;
    private readonly InboxStore _inboxStore;

    public OrderImportService(OrderImportStore importStore, InboxStore inboxStore)
    {
        _importStore = importStore;
        _inboxStore = inboxStore;
    }

    public async Task<bool> ImportAsync(MessageEnvelope<ProductCreatedV1> message, CancellationToken cancellationToken = default)
    {
        if (!_inboxStore.TryBegin(message.MessageId))
        {
            return false;
        }

        await _importStore.UpsertAsync(new ImportedProduct
        {
            Id = message.Data.ProductId,
            Name = message.Data.Name,
            Price = message.Data.Price,
            Stock = message.Data.Stock,
            SourceMessageId = message.MessageId
        }, cancellationToken);

        return true;
    }

    public Task<ImportedProduct?> GetProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _importStore.FindAsync(id, cancellationToken);
    }

    public Task Consume(ConsumeContext<MessageEnvelope<ProductCreatedV1>> context)
    {
        return ImportAsync(context.Message, context.CancellationToken);
    }
}
