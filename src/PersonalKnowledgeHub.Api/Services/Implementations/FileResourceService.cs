using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Services.Implementations;

public class FileResourceService : IFileResourceService
{
    private readonly IResourceRepository _resourceRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IFileStorage _fileStorage;
    private readonly FileUploadOptions _options;
    private readonly ILogger<FileResourceService> _logger;

    public FileResourceService(IOptions<FileUploadOptions> options,
        IResourceRepository resourceRepository, IStoredFileRepository storedFileRepository, IFileStorage fileStorage, ILogger<FileResourceService> logger)
    {
        _resourceRepository = resourceRepository;
        _storedFileRepository = storedFileRepository;
        _fileStorage = fileStorage;
        _options = options.Value;
        _logger = logger;
    }
    
    public async Task<Resource> CreateFileResource(IFormFile file, int userId, CancellationToken cancellationToken)
    {
        if (file.Length > _options.MaxFileSizeInBytes)
        {
            throw new FileSizeLimitExceededException("File size limit exceeded");
        }

        if (await _resourceRepository.IsTitleExistAsync(file.FileName, userId, cancellationToken))
        {
            throw new ConflictException("File already existed");
        }

        FileResult? fileResult = null;

        try
        {
            await using (var fileStream = file.OpenReadStream())
            {
                fileResult = await _fileStorage.SaveFile(fileStream, file.FileName, userId, cancellationToken);
            }

            Resource resource = new Resource
            {
                Title = file.FileName,
                Url = null,
                Description = null,
                ResourceType = ResourceType.File,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsDeleted = false,
                DeletedAt = null,
                DeletedBy = null,
                Version = 0
            };

            StoredFile storedFile = new StoredFile
            {
                FileName = file.FileName,
                StoredKey = fileResult.StoredKey,
                SizeInBytes = fileResult.SizeInBytes,
                ContentType = fileResult.ContentType,
                Resource = resource,
                FileFormat = fileResult.FileFormat
            };

            resource.StoredFile = storedFile;

            Resource addedResource = await _resourceRepository.AddResourceAsync(resource, cancellationToken);
            return addedResource;
        }
        catch
        {
            if (fileResult is not null)
            {
                try
                {
                    await _fileStorage.DeleteFile(fileResult.StoredKey, userId, CancellationToken.None);
                }
                catch (Exception cleanupException)
                {
                    _logger.LogError(cleanupException, "Failed to delete file {StoredKey} after database persistence failed", fileResult.StoredKey);
                }
            }

            throw;
        }
    }

    public async Task<FileDownloadResult> OpenFileResource(int resourceId, int userId, CancellationToken cancellationToken)
    {
        Resource? resource = await _resourceRepository.GetResourceByIdAsync(resourceId, cancellationToken);
        if (resource is null)
        {
            throw new NotFoundException("Resource not found");
        }
        if (resource.UserId != userId)
        {
            throw new ForbiddenException("You are not authorized to view this resource");
        }
        if (resource.StoredFile is null)
        {
            throw new NotFoundException("This resource does not contain a file");
        }

        Stream content = await _fileStorage.OpenFile(resource.StoredFile.StoredKey, userId, cancellationToken);
        FileDownloadResult result = new FileDownloadResult
        {
            Content = content,
            ContentType = resource.StoredFile.ContentType,
            FileName = resource.StoredFile.FileName
        };

        return result;
    }

    public async Task DeleteFileResourcePermanently(int resourceId, int userId, CancellationToken cancellationToken)
    {
        Resource? resource = await _resourceRepository.GetResourceByIdForPermanentDeleteAsync(resourceId, cancellationToken);
        if (resource is null)
        {
            return;
        }
        if (resource.UserId != userId)
        {
            throw new ForbiddenException("You are not authorized to delete this resource");
        }

        if (resource.StoredFile is not null)
        {
            await _fileStorage.DeleteFile(resource.StoredFile.StoredKey, userId, cancellationToken);
            await _storedFileRepository.DeleteStoredFileByStoredKeyAsync(resource.StoredFile.StoredKey, cancellationToken);
        }
        
        await _resourceRepository.CleanUpResourceByIdAsync(resourceId, cancellationToken);
    }
}