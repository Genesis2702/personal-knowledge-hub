using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.DTOs.Responses;

public class StoredFileResponseDto
{
    public required string FileName { get; set; }
    public required long SizeInBytes { get; set; }
    public required string ContentType { get; set; }
    public required int ResourceId { get; set; }
    public required FileFormat FileFormat { get; set; }
}