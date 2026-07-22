using BuildingBlocks.Integration.MassTransit.Abstractions;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.ReadModels;
using ECommerce.Services.Shared.Contracts.InternalCommands;

namespace ECommerce.Services.Catalogs.Products.Features.ProjectingProductReadModel.v1;

public sealed class ProjectProductReadModelHandler(IProductReadRepository repository)
    : IInternalCommandHandler<ProjectProductReadModel>
{
    public async Task Handle(ProjectProductReadModel command, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(new ProductReadModel(
            command.ProductId, command.Name, command.Price, command.Stock, command.CreatedAtUtc),
            cancellationToken);
    }
}
