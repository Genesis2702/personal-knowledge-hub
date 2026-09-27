using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;
using Supabase.Storage.Exceptions;
using FileOptions = Supabase.Storage.FileOptions;

namespace PersonalKnowledgeHub.Storage.Implementations.Supabase;

public class SupabaseStorage : IFileStorage
{
    private readonly global::Supabase.Client _client;
    private readonly IFileProcessor _fileProcessor;
    private readonly SupabaseStorageOptions _options;

    public SupabaseStorage(global::Supabase.Client client, IFileProcessor fileProcessor, IOptions<SupabaseStorageOptions> options)
    {
        _client = client;
        _fileProcessor = fileProcessor;
        _options = options.Value;
    }
    
    public async Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        await using ValidatedFile validatedFile =
            await _fileProcessor.ValidateAndStageAsync(fileStream, fileName, cancellationToken);
        
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.{validatedFile.Extension}";
        
        byte[] data = validatedFile.Content.ToArray();

        await _client.Storage.From(_options.BucketName).Upload(data, storedKey, new FileOptions
        {
            ContentType = validatedFile.ContentType,
            Upsert = false
        }, cancellationToken: cancellationToken);

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
        if (!SupabaseStorageValidator.IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }

        try
        {
            byte[] data = await _client.Storage.From(_options.BucketName)
                .Download(storedKey, null, cancellationToken: cancellationToken, null);

            return new MemoryStream(data);
        }
        catch (SupabaseStorageException ex) when (FailureHint.DetectReason(ex) == FailureHint.Reason.NotFound)
        {
            throw new NotFoundException("The requested file does not exist");
        }
    }

    public async Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!SupabaseStorageValidator.IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        await _client.Storage.From(_options.BucketName).Remove(storedKey);
    }
}