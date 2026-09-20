using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
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

    public S3Storage(IOptions<S3StorageOptions> options, IAmazonS3 s3Client, IFileProcessor fileProcessor)
    {
        _options = options.Value;
        _s3Client = s3Client;
        _fileProcessor = fileProcessor;
    }
    
    public async Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        await using ValidatedFile validatedFile =
            await _fileProcessor.ValidateAndStageAsync(fileStream, fileName, cancellationToken);

        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.{validatedFile.Extension}";
        string key = $"{_options.KeyPrefix}/{storedKey}";
        
        try
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            await fileTransferUtility.UploadAsync(validatedFile.Content, _options.BucketName, key,
                cancellationToken);
            
            return new FileResult
            {
                StoredKey = storedKey,
                SizeInBytes = validatedFile.SizeInBytes,
                ContentType = validatedFile.ContentType,
                FileFormat = validatedFile.FileFormat
            };
        }
        catch (AmazonS3Exception e)
        {
            throw new AmazonS3Exception($"Error encountered on server. Message:'{e.Message}' when writing an object");
        }
        catch (Exception e)
        {
            throw new Exception($"Unknown encountered on server. Message:'{e.Message}' when writing an object");
        }
    }

    public async Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        string key = $"{_options.KeyPrefix}/{storedKey}";
        
        try
        {
            GetObjectResponse response = await _s3Client.GetObjectAsync(_options.BucketName, key, cancellationToken);

            return response.ResponseStream;
        }
        catch (AmazonS3Exception e)
        {
            throw new AmazonS3Exception($"Error encountered on server. Message:'{e.Message}' when opening an object");
        }
        catch (Exception e)
        {
            throw new Exception($"Unknown encountered on server. Message:'{e.Message}' when opening an object");
        }
    }

    public async Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        string key = $"{_options.KeyPrefix}/{storedKey}";

        try
        {
            await _s3Client.DeleteObjectAsync(_options.BucketName, key, cancellationToken);
        }
        catch (AmazonS3Exception e)
        {
            throw new AmazonS3Exception($"Error encountered on server. Message:'{e.Message}' when deleting an object");
        }
        catch (Exception e)
        {
            throw new Exception($"Unknown encountered on server. Message:'{e.Message}' when deleting an object");
        }
    }
}