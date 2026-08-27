namespace PersonalKnowledgeHub.Storage.Interfaces;

public interface IFileStorage
{
    public Task<string> SaveFile(Stream fileStream, string fileName, CancellationToken cancellationToken);
    public Task<Stream> OpenFile(string storedKey, CancellationToken cancellationToken);
    public Task DeleteFile(string storedKey, CancellationToken cancellationToken);
}