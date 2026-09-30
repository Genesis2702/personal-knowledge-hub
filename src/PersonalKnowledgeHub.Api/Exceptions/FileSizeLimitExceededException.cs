namespace PersonalKnowledgeHub.Exceptions;

public class FileSizeLimitExceededException : AppException
{
    public FileSizeLimitExceededException(string message) : base(message, 413, "File Size Exceeded") { }
}