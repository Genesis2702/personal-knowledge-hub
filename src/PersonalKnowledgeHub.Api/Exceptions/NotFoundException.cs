namespace PersonalKnowledgeHub.Exceptions
{
    public sealed class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404, "Not Found") { }
    }
}
