using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;

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
    
    public Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}