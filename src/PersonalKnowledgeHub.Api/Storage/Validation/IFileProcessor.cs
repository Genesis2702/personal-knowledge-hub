namespace PersonalKnowledgeHub.Storage.Validators;

public interface IFileProcessor
{
    public Task<ValidatedFile> ValidateAndStageAsync(Stream fileStream, string fileName,
        CancellationToken cancellationToken);
}