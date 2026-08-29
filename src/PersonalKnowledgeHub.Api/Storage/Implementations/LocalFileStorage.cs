using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Storage.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _options;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options)
    {
        _options = options.Value;
    }
    
    public Task<string> SaveFile(Stream fileStream, string fileName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> OpenFile(string storedKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteFile(string storedKey, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}