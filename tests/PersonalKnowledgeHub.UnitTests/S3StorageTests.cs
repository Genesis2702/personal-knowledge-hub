using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Storage.Implementations.S3;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;
using UnsupportedMediaTypeException = PersonalKnowledgeHub.Exceptions.UnsupportedMediaTypeException;

namespace PersonalKnowledgeHub.UnitTests;

public class S3StorageTests
{
    private readonly IOptions<S3StorageOptions> _options;
    private readonly Mock<IAmazonS3> _s3Client;
    private readonly Mock<IFileProcessor> _fileProcessor;
    private readonly IFileStorage _s3Storage;

    public S3StorageTests()
    {
        _s3Client = new Mock<IAmazonS3>();
        _fileProcessor = new Mock<IFileProcessor>();
        _options = Options.Create(new S3StorageOptions
        {
            BucketName = "test-bucket",
            KeyPrefix = "test-files",
        });
        _s3Storage = new S3Storage(_options, _s3Client.Object, _fileProcessor.Object, NullLogger<S3Storage>.Instance);
    }

    [Fact]
    public async Task SaveFile_WhenFileStreamIsValid_SavesFile()
    {
        int userId = 42;

        byte[] validBytes = "%PDF-test content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(validBytes);

        string fileName = "note.pdf";

        ValidatedFile validatedFile = new ValidatedFile
        {
            Content = fileStream,
            SizeInBytes = validBytes.Length,
            Extension = "pdf",
            ContentType = "application/pdf",
            FileFormat = FileFormat.Pdf
        };

        _fileProcessor.Setup(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validatedFile);

        var result = await _s3Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);
        
        Assert.Equal(validatedFile.SizeInBytes, result.SizeInBytes);
        Assert.Equal(validatedFile.ContentType, result.ContentType);
        Assert.Equal(validatedFile.FileFormat, result.FileFormat);
        
        _fileProcessor.Verify(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()), Times.Once);
        _s3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveFile_WhenFileStreamIsNotValid_ThrowsUnsupportedMediaTypeException()
    {
        int userId = 42;
        
        byte[] invalidBytes = "%PDE-test content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(invalidBytes);
        
        string fileName = "note.pdf";

        _fileProcessor.Setup(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnsupportedMediaTypeException("This file format is not supported"));

        Func<Task> result = () => _s3Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(result);
        
        _fileProcessor.Verify(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()), Times.Once);
        _s3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task SaveFile_WhenFileSizeIsExceeded_ThrowsFileSizeExceededException()
    {
        int userId = 42;

        byte[] oversizedBytes = new byte[10 * 1024 * 1024 + 1];
        
        byte[] signatureBytes = "%PDF-test content"u8.ToArray();
        signatureBytes.CopyTo(oversizedBytes);
        
        await using Stream fileStream = new MemoryStream(oversizedBytes);
        
        string fileName = "note.pdf";

        _fileProcessor.Setup(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileSizeLimitExceededException("The requested file is too large"));

        Func<Task> result = () => _s3Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);

        await Assert.ThrowsAsync<FileSizeLimitExceededException>(result);
        
        _fileProcessor.Verify(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()), Times.Once);
        _s3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task SaveFile_WhenFileExtensionIsInvalid_ThrowsUnsupportedMediaTypeException()
    {
        int userId = 42;
        
        byte[] invalidBytes = "%PDE-test content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(invalidBytes);
        
        string fileName = "note.exe";
        
        _fileProcessor.Setup(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnsupportedMediaTypeException("This file format is not supported"));

        Func<Task> result = () => _s3Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(result);
        
        _fileProcessor.Verify(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()), Times.Once);
        _s3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task SaveFile_WhenFileNameIsInvalid_ThrowsArgumentNullException()
    {
        int userId = 42;
        
        byte[] invalidBytes = "%PDE-test content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(invalidBytes);
        
        string fileName = "";

        _fileProcessor.Setup(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentNullException());

        Func<Task> result = () => _s3Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ArgumentNullException>(result);
        
        _fileProcessor.Verify(x => x.ValidateAndStageAsync(fileStream, fileName, It.IsAny<CancellationToken>()), Times.Once);
        _s3Client.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OpenFile_WhenStoredKeyIsValid_OpensFile()
    {
        int userId = 42;
        
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.pdf";
        string key = $"{_options.Value.KeyPrefix}/{storedKey}";
        
        byte[] signatureBytes = "%PDF-test content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(signatureBytes);

        GetObjectResponse response = new GetObjectResponse
        {
            ResponseStream = fileStream
        };

        _s3Client.Setup(x =>
                x.GetObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        await using var result = await _s3Storage.OpenFile(storedKey, userId, CancellationToken.None);

        using var resultStream = new MemoryStream();
        await result.CopyToAsync(resultStream);
        
        Assert.Equal(signatureBytes, resultStream.ToArray());
        
        _s3Client.Verify(x => x.GetObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task OpenFile_WhenStoredKeyIsInvalid_ThrowsArgumentException()
    {
        int userId = 42;
        
        string invalidGuid = "random";
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{invalidGuid}.pdf";
        string key = $"{_options.Value.KeyPrefix}/{storedKey}";
        
        Func<Task> result = () => _s3Storage.OpenFile(storedKey, userId, CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(result);
        
        _s3Client.Verify(x => x.GetObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OpenFile_WhenStorageObjectDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 42;
        
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.pdf";
        string key = $"{_options.Value.KeyPrefix}/{storedKey}";

        _s3Client.Setup(x =>
                x.GetObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("The requested file does not exist"));
        
        Func<Task> result = () => _s3Storage.OpenFile(storedKey, userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _s3Client.Verify(x => x.GetObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFile_WhenStoredKeyIsValid_DeletesFile()
    {
        int userId = 42;
        
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.pdf";
        string key = $"{_options.Value.KeyPrefix}/{storedKey}";

        _s3Client.Setup(x => x.DeleteObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DeleteObjectResponse());
        
        await _s3Storage.DeleteFile(storedKey, userId, CancellationToken.None);
        
        _s3Client.Verify(x => x.DeleteObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFile_WhenStoredKeyIsInvalid_ThrowsArgumentException()
    {
        int userId = 42;

        string invalidGuid = "random";
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{invalidGuid}.pdf";
        string key = $"{_options.Value.KeyPrefix}/{storedKey}";
        
        Func<Task> result = () => _s3Storage.DeleteFile(storedKey, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ArgumentException>(result);
        
        _s3Client.Verify(x => x.DeleteObjectAsync(_options.Value.BucketName, key, It.IsAny<CancellationToken>()), Times.Never);
    }
}