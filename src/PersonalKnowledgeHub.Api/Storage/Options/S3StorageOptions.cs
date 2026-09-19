namespace PersonalKnowledgeHub.Storage.Options;

public sealed class S3StorageOptions
{
    public const string Options = "S3StorageOptions";
    
    public string BucketName { get; init; } = String.Empty;
    public string KeyPrefix { get; init; } = "files";
}