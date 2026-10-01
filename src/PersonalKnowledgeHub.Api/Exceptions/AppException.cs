namespace PersonalKnowledgeHub.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }

    protected AppException(string message, int statusCode, string title) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}