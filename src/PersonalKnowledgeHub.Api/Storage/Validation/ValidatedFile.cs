using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Storage.Validators;

public class ValidatedFile : IAsyncDisposable
{
    public required MemoryStream Content { get; init; }
    public required long SizeInBytes { get; init; }
    public required string Extension { get; init; }
    public required string ContentType { get; init; }
    public required FileFormat FileFormat { get; init; }

    public ValueTask DisposeAsync()
    {
        return Content.DisposeAsync();
    }
}