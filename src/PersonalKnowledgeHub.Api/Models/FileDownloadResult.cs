namespace PersonalKnowledgeHub.Models;

public sealed record class FileDownloadResult
{
    public required Stream Content { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
}