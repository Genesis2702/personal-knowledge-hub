using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Storage.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _storageOptions;
    private string _targetFolder;
    private string _tempoparyFolder;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> storageOptions, IOptions<FileUploadOptions> uploadOptions,  IWebHostEnvironment env)
    {
        _storageOptions = storageOptions.Value;
        
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

        if (extension != FileFormat.Pdf.ToString("G").ToLower() &&
            extension != FileFormat.Png.ToString("G").ToLower() && extension != FileFormat.Mp4.ToString("G").ToLower())
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
        if (!IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
            
        string physicalPath = Path.Combine(_targetFolder, storedKey);
        string normalizedPath = Path.GetFullPath(physicalPath);

        if (!IsFullPathValid(normalizedPath))
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
        if (!IsStoredKeyValid(storedKey, userId))
        {
            throw new ArgumentException("The requested path is invalid");
        }
        
        string physicalPath = Path.Combine(_targetFolder, storedKey);
        string normalizedPath = Path.GetFullPath(physicalPath);

        if (!IsFullPathValid(normalizedPath))
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

    private bool IsStoredKeyValid(string storedKey, int userId)
    {
        if (String.IsNullOrEmpty(storedKey)) return false;
        
        char[] separators =
        {
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        };

        string[] segments = storedKey.Split(separators);

        if (segments.Length != 4) return false;
        
        string userIdSegment = segments[0];
        string yearSegment = segments[1];
        string monthSegment = segments[2];
        
        string[] fileSegments = segments[3].Split('.');

        if (fileSegments.Length != 2) return false;

        string fileName = fileSegments[0];
        string fileExtension = fileSegments[1];

        if (Int32.TryParse(userIdSegment, out int id))
        {
            if (id != userId) return false;
        }
        else return false;

        if (Int32.TryParse(yearSegment, out int year))
        {
            if (year > DateTime.UtcNow.Year) return false;
        }
        else return false;

        if (Int32.TryParse(monthSegment, out int month))
        {
            if (year < DateTime.UtcNow.Year && (month < 1 || month > 12)) return false;
            if (year == DateTime.UtcNow.Year && (month > DateTime.UtcNow.Month || month < 1)) return false;
        }
        else return false;
        
        if (!Guid.TryParse(fileName, out Guid guid)) return false;

        fileExtension = fileExtension.ToLower();
        if (fileExtension != FileFormat.Pdf.ToString("G").ToLower() && fileExtension != FileFormat.Png.ToString("G").ToLower() && fileExtension != FileFormat.Mp4.ToString("G").ToLower()) return false;

        return true;
    }

    private bool IsFullPathValid(string normalizedPath)
    {
        StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        string explicitTargetFolder = _targetFolder.EndsWith(Path.DirectorySeparatorChar)
            ? _targetFolder
            : _targetFolder + Path.DirectorySeparatorChar;
        return normalizedPath.StartsWith(explicitTargetFolder, comparison);
    }
}