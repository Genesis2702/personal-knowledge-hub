namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Redis;

[CollectionDefinition(nameof(RedisCollection), DisableParallelization = true)]
public class RedisCollection : ICollectionFixture<RedisFixture>
{
    
}