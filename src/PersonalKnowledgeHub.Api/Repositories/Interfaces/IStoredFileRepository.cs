using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IStoredFileRepository
{
    public Task<StoredFile?> GetStoredFileByResourceId(int resourceId);
    public Task<StoredFile?> GetStoredFileByStoredKey(string storedKey);
    public Task<StoredFile?> GetStoredFileById(int id);
    public Task<StoredFile> AddStoredFile(StoredFile storedFile);
    public Task DeleteStoredFileByStoredKey(string storedKey);
}