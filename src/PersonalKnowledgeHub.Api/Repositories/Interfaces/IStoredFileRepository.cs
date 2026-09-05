using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IStoredFileRepository
{
    public Task<StoredFile?> GetStoredFileByResourceId(int resourceId, CancellationToken cancellationToken);
    public Task<StoredFile?> GetStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken);
    public Task<StoredFile?> GetStoredFileById(int id, CancellationToken cancellationToken);
    public Task<StoredFile> AddStoredFile(StoredFile storedFile, CancellationToken cancellationToken);
    public Task DeleteStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken);
}