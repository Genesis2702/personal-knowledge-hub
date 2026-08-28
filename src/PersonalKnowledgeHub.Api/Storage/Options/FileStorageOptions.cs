namespace PersonalKnowledgeHub.Storage.Options;

public class FileStorageOptions
{
    public const string Options = "FileStorageOptions";

    public string FileDirectory { get; set; } = String.Empty;
    public long FileMaxSizeLimit { get; set; }
}