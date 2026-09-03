using Order.Shared.Data;
using Tests.Shared.TestBase;

namespace Order.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public abstract class OrdersIntegrationTestBase
    : IntegrationTestBase<Program, OrdersSharedFixture>
{
    protected OrdersIntegrationTestBase(OrdersSharedFixture sharedFixture)
        : base(sharedFixture) { }

    protected override string MessagingTransport => "rabbitmq";

    protected override Task ResetStateAsync() =>
        ExecuteDbContextAsync<OrdersDbContext>(_ => Task.CompletedTask);

    protected Task ExecuteOrdersDbContextAsync(Func<OrdersDbContext, Task> action)
        => ExecuteDbContextAsync(action);

    protected Task<TResult> ExecuteOrdersDbContextAsync<TResult>(
        Func<OrdersDbContext, Task<TResult>> action)
        => ExecuteDbContextAsync<OrdersDbContext, TResult>(action);
}
