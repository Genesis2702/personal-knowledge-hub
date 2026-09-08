using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Mapper;
using PersonalKnowledgeHub.Models;
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
        StoredFile? storedFile = await _storedFileRepository.GetStoredFileByResourceIdAsync(resourceId, cancellationToken);
        if (storedFile == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        return storedFile;
    }

    public async Task<StoredFile> GetStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken)
    {
        StoredFile? storedFile = await _storedFileRepository.GetStoredFileByStoredKeyAsync(storedKey, cancellationToken);
        if (storedFile == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        return storedFile;
    }

    public async Task<StoredFile> GetStoredFileById(int id, CancellationToken cancellationToken)
    {
        StoredFile? storedFile = await _storedFileRepository.GetStoredFileByIdAsync(id, cancellationToken);
        if (storedFile == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        return storedFile;
    }

    public async Task<StoredFile> AddStoredFile(string fileName, FileResult fileResult, int resourceId,
        CancellationToken cancellationToken)
    {
        StoredFile storedFile = StoredFileMapper.ToStoredFile(fileName, fileResult, resourceId);
        StoredFile addedStoredFile = await _storedFileRepository.AddStoredFileAsync(storedFile, cancellationToken);
        return addedStoredFile;
    }

    public async Task DeleteStoredFileByStoredKey(string storedKey, CancellationToken cancellationToken)
    {
        if (await _storedFileRepository.GetStoredFileByStoredKeyForCleanupAsync(storedKey, cancellationToken) == null)
        {
            throw new NotFoundException("Stored file not found");
        }
        await _storedFileRepository.DeleteStoredFileByStoredKeyAsync(storedKey, cancellationToken);
    }
}