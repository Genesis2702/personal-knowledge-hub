namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;

public class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationFixture Fixture { get; }

    protected IntegrationTestBase(IntegrationFixture fixture)
    {
        Fixture = fixture;
    }
    
    public Task InitializeAsync()
    {
        return Fixture.ResetStateAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}