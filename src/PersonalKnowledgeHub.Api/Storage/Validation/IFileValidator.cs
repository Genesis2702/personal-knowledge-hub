namespace PersonalKnowledgeHub.Storage.Validators;

public interface IFileValidator
{
    public void ValidateFileName(string fileName);
    public void ValidateFileSignature(ReadOnlySpan<byte> signature, FileTypeDescriptor descriptor);
    public void ValidateFileSize(long fileSize);
}