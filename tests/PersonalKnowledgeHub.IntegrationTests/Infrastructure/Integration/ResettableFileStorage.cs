using System.Collections.Concurrent;
using MimeKit;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Models;
using PersonalKnowledgeHub.Storage.Interfaces;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.Integration;

public class ResettableFileStorage : IFileStorage, IResettableFileStorage
{
    private sealed record StoredFileData(byte[] content, string ContentType, FileFormat fileFormat);
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

        (string ContentType, FileFormat format) = extension switch
        {
            "pdf" => ("application/pdf", FileFormat.Pdf),
            "png" => ("image/png", FileFormat.Png),
            "mp4" => ("video/mp4", FileFormat.Mp4),
            _ => throw new InvalidOperationException(
                $"Unexpected test file extension: {extension}")
        };

        string storedKey = $"{userId}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}.{extension}";

        _files[(userId, storedKey)] = new StoredFileData(content, ContentType, format);

        return new FileResult
        {
            StoredKey = storedKey,
            SizeInBytes = content.LongLength,
            ContentType = ContentType,
            FileFormat = format
        };
    }

    public Task<Stream> OpenFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        if (!_files.TryGetValue((userId, storedKey), out var file))
        {
            throw new FileNotFoundException("The requested test file does not exist");
        }

        Stream stream = new MemoryStream(file.content, writable: false);

        return Task.FromResult(stream);
    }

    public Task DeleteFile(string storedKey, int userId, CancellationToken cancellationToken)
    {
        _files.TryRemove((userId, storedKey), out _);
        return Task.CompletedTask;
    }

    public void Reset(CancellationToken cancellationToken = default)
    {
        _files.Clear();
    }
}