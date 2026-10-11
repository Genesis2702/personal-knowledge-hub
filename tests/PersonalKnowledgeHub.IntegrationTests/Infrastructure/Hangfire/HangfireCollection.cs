namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Hangfire;

[CollectionDefinition(nameof(HangfireCollection), DisableParallelization = true)]
public class HangfireCollection : ICollectionFixture<HangfireFixture>
{
    
}