using Tests.Shared.Fixtures;

namespace AgentGetStarted.IntegrationTests;

public sealed class AgentGetStartedSharedFixture : SharedFixture<Program>
{
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();
		RequireDeepSeekApiKey();
	}
}
