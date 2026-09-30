namespace PersonalKnowledgeHub.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message) : base(message, 404, "Not Found") { }
    }
}
