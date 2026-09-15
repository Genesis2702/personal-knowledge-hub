using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Storage.Validators;

public class FileTypeDescriptor
{
    public required string Extension { get; init; }
    public required string ContentType { get; init; }
    public required FileFormat FileFormat { get; init; }
    public required byte[] Signature { get; init; }
    public required int SignatureOffset { get; init; }
    public IReadOnlySet<string> AllowedBrands { get; init; } = new HashSet<string>();
    public int BrandBytesNumber { get; init; }
}