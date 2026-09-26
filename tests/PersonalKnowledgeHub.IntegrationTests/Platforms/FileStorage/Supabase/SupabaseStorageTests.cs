using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.FileStorage.Supabase;
using PersonalKnowledgeHub.Models;

namespace PersonalKnowledgeHub.IntegrationTests.Platforms.FileStorage.Supabase;

[Collection(nameof(SupabaseStorageCollection))]
public class SupabaseStorageTests : IAsyncLifetime
{
    private readonly SupabaseStorageFixture _fixture;

    public SupabaseStorageTests(SupabaseStorageFixture fixture)
    {
        _fixture = fixture;
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.ResetAsync();
    }

    [Fact]
    public async Task SaveFile_WhenFileSignatureIsValid_SavesFile()
    {
        int userId = 42;
        string fileName = "note.pdf";

        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        FileResult result = await _fixture.Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);
        _fixture.Track(userId, result);
        
        Assert.Equal(expectedBytes.Length, result.SizeInBytes);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal(FileFormat.Pdf, result.FileFormat);
    }

    [Fact]
    public async Task SaveFile_WhenFileSignatureIsNotValid_ThrowsUnsupportedMediaTypeException()
    {
        int userId = 42;
        string fileName = "note.pdf";

        byte[] expectedBytes = "%PDE-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        Func<Task> result = () => _fixture.Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);

        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(result);
    }
    
    [Fact]
    public async Task SaveFile_WhenFileSizeExceedsMaxSize_ThrowsFileSizeLimitExceededException()
    {
        int userId = 42;
        string fileName = "note.pdf";

        byte[] fileBytes = new byte[20 * 1024 * 1024 + 10];
        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        expectedBytes.CopyTo(fileBytes, 0);
        
        await using Stream fileStream = new MemoryStream(fileBytes);

        Func<Task> result = () => _fixture.Storage.SaveFile(fileStream, fileName, userId, CancellationToken.None);

        await Assert.ThrowsAsync<FileSizeLimitExceededException>(result);
    }
    
    [Fact]
    public async Task OpenFile_WhenStoredKeyIsValid_OpensFile()
    {
        int userId = 42;

        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        FileResult result = await _fixture.Storage.SaveFile(fileStream, "note.pdf", userId, CancellationToken.None);
        _fixture.Track(userId, result);
        
        await using Stream openedFile =
            await _fixture.Storage.OpenFile(result.StoredKey, userId, CancellationToken.None);

        using var output = new MemoryStream();
        
        await openedFile.CopyToAsync(output);
        
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
    
    [Fact]
    public async Task OpenFile_WhenObjectDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 42;

        string date = DateTime.UtcNow.ToString("yyyy/MM");
        string notGuid = Guid.NewGuid().ToString("N");
        string extension = "pdf";
        
        string storedKey = $"{userId}/{date}/{notGuid}.{extension}";

        Func<Task> openedFile = () => _fixture.Storage.OpenFile(storedKey, userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(openedFile);
    }

    [Fact]
    public async Task DeleteFile_WhenStoredKeyIsValid_DeletesFile()
    {
        int userId = 42;

        byte[] expectedBytes = "%PDF-test file content"u8.ToArray();
        await using Stream fileStream = new MemoryStream(expectedBytes);

        FileResult result = await _fixture.Storage.SaveFile(fileStream, "note.pdf", userId, CancellationToken.None);
        _fixture.Track(userId, result);
        
        Assert.Equal(expectedBytes.Length, result.SizeInBytes);

        await _fixture.Storage.DeleteFile(result.StoredKey, userId, CancellationToken.None);
        
        Func<Task> openedFile = () => _fixture.Storage.OpenFile(result.StoredKey, userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(openedFile);
        _fixture.Untrack(userId, result);
    }
    
    [Fact]
    public async Task DeleteFile_WhenStoredKeyIsNotValid_ThrowsArgumentException()
    {
        int userId = 42;

        string date = DateTime.UtcNow.ToString("yyyy/MM");
        string notGuid = "random";
        string extension = "pdf";
        
        string invalidStoredKey = $"{userId}/{date}/{notGuid}.{extension}";

        Func<Task> result = () => _fixture.Storage.DeleteFile(invalidStoredKey, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ArgumentException>(result);
    }
}