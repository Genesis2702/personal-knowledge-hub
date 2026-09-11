using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.UnitTests;

public class FileResourceServiceTests
{
    private readonly Mock<IResourceRepository> _resourceRepository;
    private readonly Mock<IStoredFileRepository> _storedFileRepository;
    private readonly Mock<IFileStorage> _fileStorage;
    private readonly FileUploadOptions _options;
    private readonly IFileResourceService _fileResourceService;
    
    public FileResourceServiceTests()
    {
        _resourceRepository = new Mock<IResourceRepository>();
        _storedFileRepository = new Mock<IStoredFileRepository>();
        _fileStorage = new Mock<IFileStorage>();
        
        _options = new FileUploadOptions()
        {
            MaxFileSizeInBytes = 10 * 1024 * 1024
        };
        IOptions<FileUploadOptions> optionsWrapper = Options.Create(_options);
        
        _fileResourceService = new FileResourceService(optionsWrapper, _resourceRepository.Object,
            _storedFileRepository.Object, _fileStorage.Object, NullLogger<FileResourceService>.Instance);
    }

    [Fact]
    public async Task CreateFileResource_WhenFileIsValid_PersistsMetadata()
    {
        int userId = 42;
        byte[] bytes = CreatePdfBytes();
        IFormFile file = CreateFormFile(bytes, "note.pdf", "application/pdf");

        _resourceRepository.Setup(x => x.IsTitleExistAsync(file.FileName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _fileStorage.Setup(x => x.SaveFile(It.IsAny<Stream>(), file.FileName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileResult
            {
                StoredKey = "42/2026/09/0123456789abcdef0123456789abcdef.pdf",
                SizeInBytes = bytes.Length,
                ContentType = file.ContentType,
                FileFormat = FileFormat.Pdf
            });
        _resourceRepository.Setup(x => x.AddResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource resource, CancellationToken _) => resource);

        Resource result = await _fileResourceService.CreateFileResource(file, userId, CancellationToken.None);
        
        Assert.Equal("note.pdf", result.Title);
        Assert.Equal(ResourceType.File, result.ResourceType);
        Assert.Equal(userId, result.UserId);
        Assert.NotNull(result.StoredFile);
        Assert.Equal(bytes.Length, result.StoredFile.SizeInBytes);
        Assert.Equal("application/pdf", result.StoredFile.ContentType);
        
        _resourceRepository.Verify(x => x.IsTitleExistAsync(file.FileName, userId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(x => x.SaveFile(It.IsAny<Stream>(), file.FileName, userId, It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.AddResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFileResource_WhenSizeIsExceedsMaxFileSize_ThrowsFileSizeLimitExceededException()
    {
        int userId = 42;
        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.Length).Returns(_options.MaxFileSizeInBytes + 1);

        Func<Task> result = () => _fileResourceService.CreateFileResource(file.Object, userId, CancellationToken.None);

        await Assert.ThrowsAsync<FileSizeLimitExceededException>(result);
        
        _resourceRepository.Verify(x => x.IsTitleExistAsync(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()), Times.Never);
        _fileStorage.Verify(x => x.SaveFile(It.IsAny<Stream>(), It.IsAny<string>(), userId, It.IsAny<CancellationToken>()), Times.Never);
        _resourceRepository.Verify(x => x.AddResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateFileResource_WhenFileNameAlreadyExists_ThrowsConflictException()
    {
        int userId = 42;
        byte[] bytes = CreatePdfBytes();
        IFormFile file = CreateFormFile(bytes, "note.pdf", "application/pdf");

        _resourceRepository.Setup(x => x.IsTitleExistAsync(file.FileName, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _fileResourceService.CreateFileResource(file, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _resourceRepository.Verify(x => x.IsTitleExistAsync(file.FileName, userId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(x => x.SaveFile(It.IsAny<Stream>(), It.IsAny<string>(), userId, It.IsAny<CancellationToken>()), Times.Never);
        _resourceRepository.Verify(x => x.AddResourceAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OpenFileResource_WhenResourceIsValid_ReturnsFileStream()
    {
        int userId = 42;
        int resourceId = 10;

        byte[] fileBytes = "%PDF-test content"u8.ToArray();
        Stream fileStream = new MemoryStream(fileBytes);

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "note.pdf",
            ResourceType = ResourceType.File,
            UserId = userId,
            StoredFile = new StoredFile
            {
                FileName = "note.pdf",
                StoredKey = "42/2026/09/0123456789abcdef0123456789abcdef.pdf",
                SizeInBytes = fileBytes.Length,
                ContentType = "application/pdf",
                FileFormat = FileFormat.Pdf
            }
        };

        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);
        _fileStorage.Setup(x => x.OpenFile(resource.StoredFile.StoredKey, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileStream);

        FileDownloadResult result =
            await _fileResourceService.OpenFileResource(resourceId, userId, CancellationToken.None);
        
        Assert.Equal(fileStream, result.Content);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("note.pdf", result.FileName);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(x => x.OpenFile(resource.StoredFile.StoredKey, userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OpenFileResource_WhenResourceDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 42;
        int resourceId = 10;

        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Resource?)null);

        Func<Task> result = () => _fileResourceService.OpenFileResource(resourceId, userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(x => x.OpenFile(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OpenFileResource_WhenUserDoesNotOwnsResource_ThrowsForbiddenException()
    {
        int userId = 42;
        int resourceId = 10;

        byte[] fileBytes = "%PDF-test content"u8.ToArray();

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "note.pdf",
            ResourceType = ResourceType.File,
            UserId = 43,
            StoredFile = new StoredFile
            {
                FileName = "note.pdf",
                StoredKey = "42/2026/09/0123456789abcdef0123456789abcdef.pdf",
                SizeInBytes = fileBytes.Length,
                ContentType = "application/pdf",
                FileFormat = FileFormat.Pdf
            }
        };

        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        Func<Task> result = () => _fileResourceService.OpenFileResource(resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(x => x.OpenFile(resource.StoredFile.StoredKey, userId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task OpenFileResource_WhenResourceDoesNotContainAFile_ThrowsNotFoundException()
    {
        int userId = 42;
        int resourceId = 10;

        Resource resource = new Resource
        {
            Id = resourceId,
            Title = "note.pdf",
            ResourceType = ResourceType.File,
            UserId = userId,
        };

        _resourceRepository.Setup(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(resource);

        Func<Task> result = () => _fileResourceService.OpenFileResource(resourceId, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _resourceRepository.Verify(x => x.GetResourceByIdAsync(resourceId, It.IsAny<CancellationToken>()), Times.Once);
        _fileStorage.Verify(x => x.OpenFile(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteFileResourcePermanently_WhenCalled_CleanupExpiredResources()
    {
        int userId = 42;

        byte[] pdfBytes = CreatePdfBytes();
        
        List<Resource> resources = new()
        {
            new Resource
            {
                Title = "resource1",
                ResourceType = ResourceType.File,
                UserId = userId,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow,
                StoredFile = new StoredFile
                {
                    FileName = "note.pdf",
                    StoredKey = "42/2026/09/0123456789abcdef0123456789abcdef.pdf",
                    SizeInBytes = pdfBytes.Length,
                    ContentType = "application/pdf",
                    FileFormat = FileFormat.Pdf
                }
            },
            new Resource
            {
                Title = "resource2",
                ResourceType = ResourceType.Video,
                UserId = userId,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            }
        };

        _resourceRepository
            .SetupSequence(x => x.GetExpiredResourcesByBatchAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(resources).ReturnsAsync([]);
        _fileStorage.Setup(x => x.DeleteFile(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _storedFileRepository.Setup(x =>
                x.DeleteStoredFileByStoredKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _resourceRepository.Setup(x => x.CleanUpResourceByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _fileResourceService.DeleteFileResourcePermanently(CancellationToken.None);

        _resourceRepository
            .Verify(x => x.GetExpiredResourcesByBatchAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        _fileStorage.Verify(x => x.DeleteFile(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()), Times.Once);
        _storedFileRepository.Verify(x =>
            x.DeleteStoredFileByStoredKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepository.Verify(x => x.CleanUpResourceByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    private IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
            ContentDisposition = $"form-data; name=\"file\"; filename=\"{fileName}\""
        };
    }

    private byte[] CreatePdfBytes(int totalSize = 128)
    {
        byte[] result = new byte[totalSize];
        byte[] signature = "%PDF-"u8.ToArray();
        signature.CopyTo(result, 0);
        return result;
    }

    private byte[] CreatePngBytes(int totalSize = 128)
    {
        byte[] result = new byte[totalSize];
        byte[] signature =
        [
            0x89, 0x50, 0x4E, 0x47,
            0x0D, 0x0A, 0x1A, 0x0A
        ];
        signature.CopyTo(result, 0);
        return result;
    }

    private byte[] CreateMp4Bytes(int totalSize = 128)
    {
        byte[] result = new byte[totalSize];
        "ftyp"u8.ToArray().CopyTo(result.AsSpan(4));
        "isom"u8.ToArray().CopyTo(result.AsSpan(8));
        return result;
    }
}