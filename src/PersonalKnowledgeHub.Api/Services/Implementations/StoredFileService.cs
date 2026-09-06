using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Services.Implementations;

public class StoredFileService : IStoredFileService
{
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IFileStorage _fileStorage;
    private readonly FileUploadOptions _uploadOptions;
    private readonly ILogger<StoredFileService> _logger;

    public StoredFileService(IStoredFileRepository storedFileRepository, IFileStorage fileStorage, IOptions<FileUploadOptions> uploadOptions, IResourceRepository resourceRepository, ILogger<StoredFileService> logger)
    {
        _storedFileRepository = storedFileRepository;
        _resourceRepository = resourceRepository;
        _fileStorage = fileStorage;
        _uploadOptions = uploadOptions.Value;
        _logger = logger;
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

    public async Task<StoredFile> AddStoredFile(IFormFile formFile, int userId, int resourceId,
        CancellationToken cancellationToken)
    {
        if (formFile.Length > _uploadOptions.MaxFileSizeInBytes)
        {
            throw new FileSizeLimitExceededException("The requested file is too large");
        }

        Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId, cancellationToken);

        if (resource == null)
        {
            throw new NotFoundException("Resource not found");
        }

        if (resource.UserId != userId)
        {
            throw new ForbiddenException("You are not authorized to use this resource");
        }
        
        await using var fileStream = formFile.OpenReadStream();
        string fileName = formFile.FileName;

        FileResult? result = null;

        try
        {
            result = await _fileStorage.SaveFile(fileStream, fileName, userId, cancellationToken);

            StoredFile storedFile = new StoredFile
            {
                StoredKey = result.StoredKey,
                SizeInBytes = result.SizeInBytes,
                ContentType = result.ContentType,
                ResourceId = resourceId,
                FileFormat = result.FileFormat
            };

            await _storedFileRepository.AddStoredFileAsync(storedFile, cancellationToken);

            return storedFile;
        }
        catch
        {
            if (result is not null)
            {
                try
                {
                    await _fileStorage.DeleteFile(result.StoredKey, userId, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(cleanupException,
                        "Failed to remove orphaned file {StoredKey} after saving its database record failed",
                        result.StoredKey);
                }
            }

            throw;
        }
    }

    public async Task DeleteStoredFileByStoredKey(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (await _storedFileRepository.GetStoredFileByStoredKeyForCleanupAsync(storedKey, cancellationToken) == null)
        {
            throw new NotFoundException("Stored file not found");
        }

        await _fileStorage.DeleteFile(storedKey, userId, cancellationToken);
        
        await _storedFileRepository.DeleteStoredFileByStoredKeyAsync(storedKey, cancellationToken);
    }
}