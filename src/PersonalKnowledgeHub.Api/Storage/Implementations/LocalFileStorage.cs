using Microsoft.Extensions.Options;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.Storage.Implementations;

public class LocalFileStorage : IFileStorage
{
    private readonly LocalFileStorageOptions _storageOptions;
    private readonly HashSet<string> _allowedExtensions;
    private readonly Dictionary<string, string> _contentTypes;
    private string _targetFolder;
    private string _tempoparyFolder;

    public LocalFileStorage(IOptions<LocalFileStorageOptions> storageOptions,  IWebHostEnvironment env)
    {
        _storageOptions = storageOptions.Value;

        _allowedExtensions = new()
        {
            FileFormat.Pdf.ToString("G").ToLower(),
            FileFormat.Png.ToString("G").ToLower(),
            FileFormat.Mp4.ToString("G").ToLower(),
        };

        _contentTypes = new()
        {
            { "pdf", "application/pdf" },
            { "png", "image/png" },
            { "mp4", "video/mp4" },
        };
        
        _targetFolder = Path.Combine(env.ContentRootPath, _storageOptions.StorageDirectory);
        _targetFolder = Path.GetFullPath(_targetFolder);
        Directory.CreateDirectory(_targetFolder);

        _tempoparyFolder = Path.Combine(env.ContentRootPath, _storageOptions.TempStorageDirectory);
        _tempoparyFolder = Path.GetFullPath(_tempoparyFolder);
        Directory.CreateDirectory(_tempoparyFolder);
    }
    
    public async Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId,
        CancellationToken cancellationToken)
    {
        string extension = Path.GetExtension(fileName);
        string guid = Guid.NewGuid().ToString("N");
        string date = DateTime.UtcNow.ToString("yyyy/MM");

        string trimmedExtension = extension.TrimStart('.');

        if (!_allowedExtensions.Contains(trimmedExtension))
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

        FileSignatures.FileSignature.TryGetValue(trimmedExtension, out var rule);
        FileSignatures.FileBrandBytes.TryGetValue(trimmedExtension, out int signatureBrandBytes);
        int signatureOffset = rule.Offset;
        int signatureBytesLength = rule.Signature.Length;

        long totalBytesRead = 0;
        int signatureBytesCollected = 0;
        bool signatureValidated = false;
        try
        {
            await using (var tempStream = File.Create(temporaryPath))
            {
                byte[] buffer = new byte[8192];
                byte[] signatureBuffer = new byte[signatureOffset + signatureBytesLength + signatureBrandBytes];
                while (true)
                {
                    int bytesRead = await fileStream.ReadAsync(buffer, cancellationToken);

                    if (bytesRead == 0) break;

                    int bytesToCopy = Math.Min(bytesRead, signatureBuffer.Length - signatureBytesCollected);
                    buffer.AsSpan(0, bytesToCopy).CopyTo(signatureBuffer.AsSpan(signatureBytesCollected));
                    signatureBytesCollected += bytesToCopy;

                    if (!signatureValidated && signatureBytesCollected == signatureBuffer.Length)
                    {
                        if (!LocalFileStorageValidators.IsFileSignatureValid(trimmedExtension, signatureBuffer))
                        {
                            throw new UnsupportedMediaTypeException("This file format is not supported");
                        }
                        signatureValidated = true;
                    }
                    
                    totalBytesRead += bytesRead;

                    if (totalBytesRead > _storageOptions.MaxStoredFileSizeInBytes)
                    {
                        throw new FileSizeLimitExceededException("The requested file is too large");
                    }

                    await tempStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }
            }

            if (!signatureValidated)
            {
                throw new UnsupportedMediaTypeException("This file format is not supported");
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

        _contentTypes.TryGetValue(trimmedExtension, out var contentType);
        Enum.TryParse<FileFormat>(trimmedExtension, true, out var fileFormat);
        
        return new FileResult
        {
            StoredKey = storedKey,
            SizeInBytes = totalBytesRead,
            ContentType = contentType!,
            FileFormat = fileFormat
        };
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

        try
        {
            Stream result = new FileStream(normalizedPath, FileMode.Open, FileAccess.Read);
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

        File.Delete(normalizedPath);

        return Task.CompletedTask;
    }
}