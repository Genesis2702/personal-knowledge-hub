using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class StoredFileService : IStoredFileService
{
    private readonly IStoredFileRepository _storedFileRepository;

    public StoredFileService(IStoredFileRepository storedFileRepository)
    {
        _storedFileRepository = storedFileRepository;
    }
    
    public async Task<StoredFile> GetStoredFileByResourceId(int resourceId, CancellationToken cancellationToken)
    {
        StoredFile? storedFile = await _storedFileRepository.GetStoredFileByResourceId(resourceId, cancellationToken);
        if (storedFile == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        return storedFile;
    }

    public async Task<StoredFile> GetStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken)
    {
        StoredFile? storedFile = await _storedFileRepository.GetStoredFileByStoredKey(storedKey, cancellationToken);
        if (storedFile == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        return storedFile;
    }

    public async Task<StoredFile> GetStoredFileById(int id, CancellationToken cancellationToken)
    {
        StoredFile? storedFile = await _storedFileRepository.GetStoredFileById(id, cancellationToken);
        if (storedFile == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        return storedFile;
    }

    public Task<StoredFile> AddStoredFile(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken)
    {
        if (await _storedFileRepository.GetStoredFileByStoredKey(storedKey, cancellationToken) == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        await _storedFileRepository.DeleteStoredFileByStoredKey(storedKey, cancellationToken);
    }
}