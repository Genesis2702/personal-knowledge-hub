namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;

public interface IResettableCache
{
    Task ResetAsync(CancellationToken cancellationToken = default);
}