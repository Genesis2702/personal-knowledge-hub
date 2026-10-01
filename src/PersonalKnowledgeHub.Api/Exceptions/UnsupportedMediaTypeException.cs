namespace PersonalKnowledgeHub.Exceptions;

public sealed class UnsupportedMediaTypeException : AppException
{
    public UnsupportedMediaTypeException(string message) : base(message,415, "Unsupported Media Type") { }
}