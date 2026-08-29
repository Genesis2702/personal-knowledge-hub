namespace PersonalKnowledgeHub.Storage.Options;

public class FileUploadOptions
{
    public const string Options = "FileUploadOptions";
    
    public long MaxFileSizeInBytes { get; init; }
}