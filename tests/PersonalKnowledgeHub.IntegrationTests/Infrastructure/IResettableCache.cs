namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure;

public interface IResettableCache
{
    Task ResetAsync(CancellationToken cancellationToken = default);
}