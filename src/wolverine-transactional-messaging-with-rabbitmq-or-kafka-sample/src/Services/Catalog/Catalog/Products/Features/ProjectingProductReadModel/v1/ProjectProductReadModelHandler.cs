using Catalog.Products.Models;
using Catalog.Shared.ReadModels;
using Contracts.Messages.ProjectProductReadModel;
using MediatR;

namespace Catalog.Products.Features.ProjectingProductReadModel.v1;

public sealed class ProjectProductReadModelHandler(IProductReadRepository repository)
    : IRequestHandler<ProjectProductReadModel>
{
    public async Task Handle(ProjectProductReadModel command, CancellationToken cancellationToken)
    {
        await repository.UpsertAsync(new ProductReadModel(
            command.ProductId, command.Name, command.Category,
            command.Description, command.Price, DateTime.UtcNow),
            cancellationToken);
    }
}
