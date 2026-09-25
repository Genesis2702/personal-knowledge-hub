using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;

namespace PersonalKnowledgeHub.Storage.Implementations.Supabase;

public class SupabaseStorage : IFileStorage
{
    private readonly global::Supabase.Client _client;

    public SupabaseStorage(global::Supabase.Client client)
    {
        _client = client;
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