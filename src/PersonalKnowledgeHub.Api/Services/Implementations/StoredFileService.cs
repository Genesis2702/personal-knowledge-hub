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

    public StoredFileService(IStoredFileRepository storedFileRepository, IFileStorage fileStorage, IOptions<FileUploadOptions> uploadOptions, IResourceRepository resourceRepository)
    {
        _storedFileRepository = storedFileRepository;
        _resourceRepository = resourceRepository;
        _fileStorage = fileStorage;
        _uploadOptions = uploadOptions.Value;
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

        FileResult result = await _fileStorage.SaveFile(fileStream, fileName, userId, cancellationToken);

        StoredFile storedFile = new StoredFile
        {
            StoredKey = result.StoredKey,
            SizeInBytes = result.SizeInBytes,
            ContentType = result.ContentType,
            ResourceId = resourceId,
            FileFormat = result.FileFormat
        };
        
        await _storedFileRepository.AddStoredFile(storedFile, cancellationToken);
        
        return storedFile;
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