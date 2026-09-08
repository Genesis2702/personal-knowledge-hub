using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Models;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IStoredFileService
{
    public Task<StoredFile> GetStoredFileByResourceId(int resourceId, CancellationToken cancellationToken);
    public Task<StoredFile> GetStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken);
    public Task<StoredFile> GetStoredFileById(int id, CancellationToken cancellationToken);
    public Task<StoredFile> AddStoredFile(string fileName, FileResult fileResult, int resourceId, CancellationToken cancellationToken);
    public Task DeleteStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken);
}