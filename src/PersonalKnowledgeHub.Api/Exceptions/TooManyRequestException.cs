namespace PersonalKnowledgeHub.Exceptions
{
    public sealed class TooManyRequestException : AppException
    {
        public TooManyRequestException(string message) : base(message, 429, "Too Many Requests") { }
    }
}
