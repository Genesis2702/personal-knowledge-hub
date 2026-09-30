namespace PersonalKnowledgeHub.Exceptions
{
    public class TooManyRequestException : AppException
    {
        public TooManyRequestException(string message) : base(message, 429, "Too Many Requests") { }
    }
}
