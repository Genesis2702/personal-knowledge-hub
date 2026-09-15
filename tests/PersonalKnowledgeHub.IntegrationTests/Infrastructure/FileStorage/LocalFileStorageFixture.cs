using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using PersonalKnowledgeHub.Storage.Implementations.Local;
using PersonalKnowledgeHub.Storage.Interfaces;
using PersonalKnowledgeHub.Storage.Options;
using PersonalKnowledgeHub.Storage.Validators;

namespace PersonalKnowledgeHub.IntegrationTests.Infrastructure.FileStorage;

public sealed class LocalFileStorageFixture : IAsyncLifetime
{
    private readonly string _rootDirectory;

    public string FilesDirectory => Path.Combine(_rootDirectory, "files");
    public string TempDirectory => Path.Combine(_rootDirectory, "temp");
    
    public IFileStorage Storage { get; private set; } = null!;

    public LocalFileStorageFixture()
    {
        _rootDirectory = Path.Combine(
            Path.GetTempPath(),
            "PersonalKnowledgeHubFileStorageTests",
            Guid.NewGuid().ToString("N"));
    }
    
    public Task InitializeAsync()
    {
        Directory.CreateDirectory(_rootDirectory);

        var environment = new TestWebHostEnvironment
        {
            ContentRootPath = _rootDirectory
        };

        var storageOptions = Microsoft.Extensions.Options.Options.Create(
            new LocalFileStorageOptions
            {
                StorageDirectory = "files",
                TempStorageDirectory = "temp",
            });
        
        var fileOptions = Microsoft.Extensions.Options.Options.Create(
            new FileUploadOptions
            {
                MaxFileSizeInBytes = 1024,
                MaxStoredFileSizeInBytes = 1024
            });

        IFileValidator validator = new FileValidator(fileOptions);
        IFileProcessor processor = new FileProcessor(validator);

        Storage = new LocalFileStorage(
            storageOptions,
            environment, processor);

        return Task.CompletedTask;
    }
    
    public Task ResetAsync()
    {
        if (Directory.Exists(FilesDirectory))
        {
            Directory.Delete(
                FilesDirectory,
                recursive: true);
        }

        if (Directory.Exists(TempDirectory))
        {
            Directory.Delete(
                TempDirectory,
                recursive: true);
        }

        Directory.CreateDirectory(FilesDirectory);
        Directory.CreateDirectory(TempDirectory);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_rootDirectory))
        {
            Directory.Delete(
                _rootDirectory,
                recursive: true);
        }

        return Task.CompletedTask;
    }
    
    private sealed class TestWebHostEnvironment
        : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } =
            "PersonalKnowledgeHub.IntegrationTests";

        public string EnvironmentName { get; set; } =
            "IntegrationTesting";

        public string WebRootPath { get; set; } =
            string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } =
            new NullFileProvider();

        public required string ContentRootPath { get; set; }

        public IFileProvider ContentRootFileProvider { get; set; } =
            new NullFileProvider();
    }
}