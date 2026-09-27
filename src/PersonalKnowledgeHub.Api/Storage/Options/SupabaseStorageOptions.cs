namespace PersonalKnowledgeHub.Storage.Options;

public class SupabaseStorageOptions
{
    public const string Options = "SupabaseStorageOptions";
    public string BucketName { get; init; } = String.Empty;
}