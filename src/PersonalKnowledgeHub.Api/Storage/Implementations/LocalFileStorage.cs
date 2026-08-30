using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Storage.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _options;
    private string _targetFolder;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options)
    {
        _options = options.Value;
        _targetFolder = _options.StorageDirectory;
    }
    
    public async Task<string> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        string extension = Path.GetExtension(fileName);
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}{extension}";
        
        string physicalPath = Path.Combine(_targetFolder, storedKey);

        string directoryPath = Path.GetDirectoryName(physicalPath)!;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        await using (var targetStream = File.Create(physicalPath))
        {
            await fileStream.CopyToAsync(targetStream, cancellationToken);
        }

        return storedKey;
    }

    public async Task<FileStream> OpenFile(string storedKey, CancellationToken cancellationToken)
    {
        string physicalPath = Path.Combine(_targetFolder, storedKey);

        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException("The requested file does not exist");
        }
        
        return new FileStream(physicalPath, FileMode.Open, FileAccess.Read);
    }

    public async Task DeleteFile(string storedKey, CancellationToken cancellationToken)
    {
        string physicalPath = Path.Combine(_targetFolder, storedKey);

        if (!File.Exists(physicalPath))
        {
            throw new FileNotFoundException("The requested file does not exist");
        }

        File.Delete(physicalPath);
    }
}