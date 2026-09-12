using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.FileStorage;
using PersonalKnowledgeHub.Models;

namespace PersonalKnowledgeHub.IntegrationTests.Platforms.FileStorage;

[Collection(nameof(LocalFileStorageCollection))]
public class LocalFileStorageTests : IAsyncLifetime
{
    private readonly LocalFileStorageFixture _fixture;
    
    public LocalFileStorageTests(LocalFileStorageFixture fixture)
    {
        _fixture = fixture;
    }
    
    public Task InitializeAsync()
    {
        return _fixture.ResetAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SaveFile_WhenFileIsValid_SavesFile()
    {
        int userId = 42;

        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        FileResult result = await _fixture.Storage.SaveFile(fileStream, "note.pdf", userId, CancellationToken.None);
        
        Assert.Equal(expectedBytes.LongLength, result.SizeInBytes);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(FileFormat.Pdf, result.FileFormat);

        string physicalPath = Path.Combine(_fixture.FilesDirectory, result.StoredKey);
        
        Assert.True(File.Exists(physicalPath));
        
        byte[] savedBytes = await File.ReadAllBytesAsync(physicalPath);
        
        Assert.Equal(expectedBytes, savedBytes);
    }

    [Fact]
    public async Task SaveFile_WhenFileSignatureIsInvalid_ThrowsUnsupportedMediaTypeException()
    {
        int userId = 42;

        byte[] expectedBytes = "%PDE-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        Func<Task> result = () => _fixture.Storage.SaveFile(fileStream, "note.pdf", userId, CancellationToken.None);

        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(result);
        
        Assert.Empty(Directory.EnumerateFiles(_fixture.FilesDirectory, "*", SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(_fixture.TempDirectory, "*.*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task SaveFile_WhenFileExtensionIsInvalid_ThrowsUnsupportedMediaTypeException()
    {
        int userId = 42;

        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        Func<Task> result = () => _fixture.Storage.SaveFile(fileStream, "note.exe", userId, CancellationToken.None);

        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(result);
        
        Assert.Empty(Directory.EnumerateFiles(_fixture.FilesDirectory, "*", SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(_fixture.TempDirectory, "*.*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task SaveFile_WhenFileExceedsMaximumSize_ThrowsFileSizeLimitExceededException()
    {
        int userId = 42;
        int maximumSize = 1024;

        byte[] fileBytes = new byte[maximumSize + 1];
        
        byte[] signatureBytes = "%PDF-test file content"u8.ToArray();
        signatureBytes.CopyTo(fileBytes, 0);

        await using var fileStream = new MemoryStream(fileBytes);

        Func<Task> result = () => _fixture.Storage.SaveFile(fileStream, "note.pdf", userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<FileSizeLimitExceededException>(result);
        
        Assert.Empty(Directory.EnumerateFiles(_fixture.FilesDirectory, "*", SearchOption.AllDirectories));
        Assert.Empty(Directory.EnumerateFiles(_fixture.TempDirectory, "*.*", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task OpenFile_WhenFileIsValid_OpensFile()
    {
        int userId = 42;

        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        FileResult result = await _fixture.Storage.SaveFile(fileStream, "note.pdf", userId, CancellationToken.None);

        await using Stream openedFile =
            await _fixture.Storage.OpenFile(result.StoredKey, userId, CancellationToken.None);

        using var output = new MemoryStream();
        
        await  openedFile.CopyToAsync(output);
        
        Assert.Equal(expectedBytes, output.ToArray());
    }

    [Fact]
    public async Task OpenFile_WhenStoredKeyIsNotValid_ThrowsArgumentException()
    {
        int userId = 42;

        string date = DateTime.UtcNow.ToString("yyyy/MM");
        string notGuid = "random";
        string extension = "pdf";
        
        string invalidStoredKey = $"{userId}/{date}/{notGuid}.{extension}";

        Func<Task> openedFile = () => _fixture.Storage.OpenFile(invalidStoredKey, userId, CancellationToken.None);

        await Assert.ThrowsAsync<ArgumentException>(openedFile);
    }
}