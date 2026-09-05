using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IStoredFileRepository
{
    public Task<StoredFile?> GetStoredFileByResourceIdAsync(int resourceId, CancellationToken cancellationToken);
    public Task<StoredFile?> GetStoredFileByStoredKeyAsync(string storedKey, CancellationToken cancellationToken);
    public Task<StoredFile?> GetStoredFileByIdAsync(int id, CancellationToken cancellationToken);
    public Task<StoredFile> AddStoredFileAsync(StoredFile storedFile, CancellationToken cancellationToken);
    public Task DeleteStoredFileByStoredKeyAsync(string storedKey, CancellationToken cancellationToken);
}