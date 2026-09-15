using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.Storage.Implementations.Local;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _storageOptions;
    private readonly IFileProcessor _fileProcessor;
    private string _targetFolder;
    private string _temporaryFolder;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> storageOptions,  IWebHostEnvironment env, IFileProcessor fileProcessor)
    {
        _fileProcessor = fileProcessor;
        _storageOptions = storageOptions.Value;
        
        _targetFolder = Path.Combine(env.ContentRootPath, _storageOptions.StorageDirectory);
        _targetFolder = Path.GetFullPath(_targetFolder);
        Directory.CreateDirectory(_targetFolder);

        _temporaryFolder = Path.Combine(env.ContentRootPath, _storageOptions.TempStorageDirectory);
        _temporaryFolder = Path.GetFullPath(_temporaryFolder);
        Directory.CreateDirectory(_temporaryFolder);
    }
    
    public async Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId,
        CancellationToken cancellationToken)
    {
        await using ValidatedFile validatedFile = await _fileProcessor.ValidateAndStageAsync(fileStream, fileName, cancellationToken);
        
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string storedKey = $"{userId}/{date}/{guid}.{validatedFile.Extension}";
        
        string targetPath = Path.Combine(_targetFolder, storedKey);
        string temporaryPath = Path.Combine(_temporaryFolder, storedKey);

        string targetDirectoryPath = Path.GetDirectoryName(targetPath)!;
        string tempDirectoryPath = Path.GetDirectoryName(temporaryPath)!;

        Directory.CreateDirectory(targetDirectoryPath);
        Directory.CreateDirectory(tempDirectoryPath);

        try
        {
            await using (var temporaryStream = File.Create(temporaryPath))
            {
                await validatedFile.Content.CopyToAsync(temporaryStream, cancellationToken);
                await temporaryStream.FlushAsync(cancellationToken);
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
        
        return new FileResult
        {
            StoredKey = storedKey,
            SizeInBytes = validatedFile.SizeInBytes,
            ContentType = validatedFile.ContentType,
            FileFormat = validatedFile.FileFormat
        };
    }

    public Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!LocalFileStorageValidator.IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
            
        string physicalPath = Path.Combine(_targetFolder, storedKey);
        string normalizedPath = Path.GetFullPath(physicalPath);

        if (!LocalFileStorageValidator.IsFullPathValid(normalizedPath, _targetFolder))
        {
            throw new ArgumentException("The requested path is invalid");
        }

        try
        {
            Stream result = new FileStream(normalizedPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult(result);
        }
        catch (FileNotFoundException)
        {
            throw new FileNotFoundException("The requested file does not exist");
        }
        catch (DirectoryNotFoundException)
        {
            throw new DirectoryNotFoundException("The requested directory does not exist");
        }
    }

    public Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!LocalFileStorageValidator.IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        string physicalPath = Path.Combine(_targetFolder, storedKey);
        string normalizedPath = Path.GetFullPath(physicalPath);

        if (!LocalFileStorageValidator.IsFullPathValid(normalizedPath, _targetFolder))
        {
            throw new ArgumentException("The requested path is invalid");
        }

        File.Delete(normalizedPath);

        return Task.CompletedTask;
    }
}