using MassTransit;

namespace ECommerce.Services.Orders.Products.Features.ConsumingProductCreated.v1;

public sealed class ProductCreatedConsumerDefinition : ConsumerDefinition<ProductCreatedConsumer>
{
    public ProductCreatedConsumerDefinition()
    {
        EndpointName = "orders-products";
    }

    protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<ProductCreatedConsumer> consumerConfigurator, IRegistrationContext context)
    {
        // Policies applied centrally via ApplyEndpointPolicies
    }
}
