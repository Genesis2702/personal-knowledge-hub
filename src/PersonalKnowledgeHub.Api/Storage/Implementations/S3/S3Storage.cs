using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.Storage.Implementations.S3;

public class S3Storage : IFileStorage
{
    private readonly S3StorageOptions _options;
    private readonly IAmazonS3 _s3Client;
    private readonly IFileProcessor _fileProcessor;
    private readonly ILogger<S3Storage> _logger;

    public S3Storage(IOptions<S3StorageOptions> options, IAmazonS3 s3Client, IFileProcessor fileProcessor, ILogger<S3Storage> logger)
    {
        _options = options.Value;
        _s3Client = s3Client;
        _fileProcessor = fileProcessor;
        _logger = logger;
    }
    
    public async Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        await using ValidatedFile validatedFile =
            await _fileProcessor.ValidateAndStageAsync(fileStream, fileName, cancellationToken);

        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.{validatedFile.Extension}";
        string key = $"{_options.KeyPrefix}/{storedKey}";

        PutObjectRequest objectRequest = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            ContentType = validatedFile.ContentType,
            InputStream = validatedFile.Content,
            Key = key,
        };

        await _s3Client.PutObjectAsync(objectRequest, cancellationToken);

        return new FileResult
        {
            StoredKey = storedKey,
            SizeInBytes = validatedFile.SizeInBytes,
            ContentType = validatedFile.ContentType,
            FileFormat = validatedFile.FileFormat
        };
    }

    public async Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!S3StorageValidator.IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        string key = $"{_options.KeyPrefix}/{storedKey}";

        try
        {
            using GetObjectResponse response =
                await _s3Client.GetObjectAsync(_options.BucketName, key, cancellationToken);
            
            Stream fileStream = new MemoryStream();
            try
            {
                await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);
                fileStream.Position = 0;

                return fileStream;
            }
            catch
            {
                await fileStream.DisposeAsync();
                throw;
            }
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound && ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning(ex, "S3 object {ObjectKey} was not found in bucket {BucketName}", key, _options.BucketName);
            throw new NotFoundException("The requested file does not exist");
        }
    }

    public async Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!S3StorageValidator.IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        string key = $"{_options.KeyPrefix}/{storedKey}";

        await _s3Client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
    }
}