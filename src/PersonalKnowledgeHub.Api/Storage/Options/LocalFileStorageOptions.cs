namespace PersonalKnowledgeHub.Storage.Options;

public class LocalFileStorageOptions
{
    public const string Options = "LocalFileStorageOptions";

    public string StorageDirectory { get; init; } = String.Empty;
    public long MaxStoredFileSizeInBytes { get; init; }
}