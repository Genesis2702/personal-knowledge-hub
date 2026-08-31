using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;

namespace PersonalKnowledgeHub.Storage.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _options;
    private string _targetFolder;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> options, IWebHostEnvironment env)
    {
        _options = options.Value;
        _targetFolder = Path.Combine(env.ContentRootPath, _options.StorageDirectory);
        _targetFolder = Path.GetFullPath(_targetFolder);
        Directory.CreateDirectory(_targetFolder);
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

    public async Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
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
        
        return new FileStream(normalizedPath, FileMode.Open, FileAccess.Read);
    }

    public async Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
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