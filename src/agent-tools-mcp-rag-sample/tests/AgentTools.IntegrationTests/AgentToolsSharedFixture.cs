using Tests.Shared.Fixtures;

namespace AgentTools.IntegrationTests;

public sealed class AgentToolsSharedFixture : SharedFixture<Program>
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        RequireDeepSeekApiKey();
    }
}
