namespace PersonalKnowledgeHub.Exceptions
{
    public sealed class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message) : base(message, 401, "Unauthorized") { }
    }
}
