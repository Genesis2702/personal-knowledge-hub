namespace PersonalKnowledgeHub.Storage.Interfaces;

public interface IFileStorage
{
    public Task<string> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken);
    public Task<FileStream> OpenFile(string storedKey, CancellationToken cancellationToken);
    public Task DeleteFile(string storedKey, CancellationToken cancellationToken);
}