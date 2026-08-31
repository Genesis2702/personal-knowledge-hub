namespace PersonalKnowledgeHub.Storage.Interfaces;

public interface IFileStorage
{
    public Task<string> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken);
    public Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken);
    public Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken);
}