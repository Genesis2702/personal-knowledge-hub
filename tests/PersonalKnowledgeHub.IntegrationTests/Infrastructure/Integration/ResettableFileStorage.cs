using System.Collections.Concurrent;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;

public class ResettableFileStorage : IFileStorage, IResettableFileStorage
{
    private sealed record StoredFileData(byte[] Content, string ContentType, FileFormat FileFormat);
    private readonly ConcurrentDictionary<(int UserId, string StoredKey), StoredFileData> _files = new();
    
    public async Task<FileResult> SaveFile(Stream fileStream, string fileName, int userId, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();

        await fileStream.CopyToAsync(buffer, cancellationToken);

        byte[] content = buffer.ToArray();
        string extension = Path
            .GetExtension(fileName)
            .TrimStart('.')
            .ToLowerInvariant();

        (string contentType, FileFormat format) = extension switch
        {
            "pdf" => ("application/pdf", FileFormat.Pdf),
            "png" => ("image/png", FileFormat.Png),
            "mp4" => ("video/mp4", FileFormat.Mp4),
            _ => throw new UnsupportedMediaTypeException(
                $"Unexpected test file extension: {extension}")
        };

        string storedKey = $"{userId}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}.{extension}";

        _files[(userId, storedKey)] = new StoredFileData(content, contentType, format);

        return new FileResult
        {
            StoredKey = storedKey,
            SizeInBytes = content.LongLength,
            ContentType = contentType,
            FileFormat = format
        };
    }

    public Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!_files.TryGetValue((userId, storedKey), out var file))
        {
            throw new FileNotFoundException("The requested test file does not exist");
        }

        Stream stream = new MemoryStream(file.Content, writable: false);

        return Task.FromResult(stream);
    }

    public Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        _files.TryRemove((userId, storedKey), out _);
        return Task.CompletedTask;
    }

    public void Reset()
    {
        _files.Clear();
    }
}