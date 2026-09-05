namespace PersonalKnowledgeHub.Models;

public record FileResult()
{
    public required string StoredKey { get; init; }
    public required long SizeInBytes { get; init; }
    public required string ContentType { get; init; }
}