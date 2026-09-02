using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.Storage.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _storageOptions;
    private readonly HashSet<string> _allowedExtensions;
    private string _targetFolder;
    private string _tempoparyFolder;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> storageOptions, IOptions<FileUploadOptions> uploadOptions,  IWebHostEnvironment env)
    {
        _storageOptions = storageOptions.Value;

        _allowedExtensions = new()
        {
            FileFormat.Pdf.ToString("G").ToLower(),
            FileFormat.Png.ToString("G").ToLower(),
            FileFormat.Mp4.ToString("G").ToLower(),
        };
        
        _targetFolder = Path.Combine(env.ContentRootPath, _storageOptions.StorageDirectory);
        _targetFolder = Path.GetFullPath(_targetFolder);
        Directory.CreateDirectory(_targetFolder);

        _tempoparyFolder = Path.Combine(env.ContentRootPath, _storageOptions.TempStorageDirectory);
        _tempoparyFolder = Path.GetFullPath(_tempoparyFolder);
        Directory.CreateDirectory(_tempoparyFolder);
    }
    
    public async Task<string> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        string extension = Path.GetExtension(fileName);
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        if (!_allowedExtensions.Contains(extension))
        {
            throw new UnsupportedMediaTypeException("This file format is not supported");
        }

        string storedKey = $"{userId}/{date}/{guid}{extension}";
        
        string targetPath = Path.Combine(_targetFolder, storedKey);
        string temporaryPath = Path.Combine(_tempoparyFolder, storedKey);

        string targetDirectoryPath = Path.GetDirectoryName(targetPath)!;
        string tempDirectoryPath = Path.GetDirectoryName(temporaryPath)!;

        Directory.CreateDirectory(targetDirectoryPath);
        Directory.CreateDirectory(tempDirectoryPath);

        long totalBytesRead = 0;
        try
        {
            await using (var tempStream = File.Create(temporaryPath))
            {
                byte[] buffer = new byte[8192];
                while (true)
                {
                    int bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);

                    if (bytesRead == 0) break;
                    
                    totalBytesRead += bytesRead;

                    if (totalBytesRead > _storageOptions.MaxStoredFileSizeInBytes)
                    {
                        throw new FileSizeLimitExceededException("The requested file is too large");
                    }

                    await tempStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }
            }
            
            File.Move(temporaryPath, targetPath);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }

        return storedKey;
    }

    public Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!LocalFileStorageValidators.IsStoredKeyValid(storedKey, userId, _allowedExtensions))
        {
            throw new ArgumentException("The requested path is invalid");
        }
            
        string physicalPath = Path.Combine(_targetFolder, storedKey);
        string normalizedPath = Path.GetFullPath(physicalPath);

        if (!LocalFileStorageValidators.IsFullPathValid(normalizedPath, _targetFolder))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException("The requested file does not exist");
        }

        Stream result = new FileStream(normalizedPath, FileMode.Open, FileAccess.Read);

        return Task.FromResult(result);
    }

    public Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!LocalFileStorageValidators.IsStoredKeyValid(storedKey, userId, _allowedExtensions))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        string physicalPath = Path.Combine(_targetFolder, storedKey);
        string normalizedPath = Path.GetFullPath(physicalPath);

        if (!LocalFileStorageValidators.IsFullPathValid(normalizedPath, _targetFolder))
        {
            throw new ArgumentException("The requested path is invalid");
        }

        if (!File.Exists(normalizedPath))
        {
            throw new FileNotFoundException("The requested file does not exist");
        }

        File.Delete(normalizedPath);

        return Task.CompletedTask;
    }
}