namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;

public interface IResettableFileStorage
{
    void Reset(CancellationToken cancellationToken = default);
}