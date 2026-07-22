using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.ReadModels;
using ECommerce.Services.Shared.Contracts.InternalCommands;
using MassTransit;

namespace ECommerce.Services.Catalogs.Products.Features.ProjectingProductReadModel.v1;

public sealed class ProjectProductReadModelConsumer(IProductReadRepository repository)
    : IConsumer<ProjectProductReadModel>
{
    public Task Consume(ConsumeContext<ProjectProductReadModel> context)
    {
        var msg = context.Message;
        return repository.UpsertAsync(new ProductReadModel(
            msg.ProductId, msg.Name, msg.Price, msg.Stock, msg.CreatedAtUtc), context.CancellationToken);
    }
}
