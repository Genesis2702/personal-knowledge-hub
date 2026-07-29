namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationFixture Fixture { get; }

    protected IntegrationTestBase(IntegrationFixture fixture)
    {
        Fixture = fixture;
    }
    
    public Task InitializeAsync()
    {
        return Fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}