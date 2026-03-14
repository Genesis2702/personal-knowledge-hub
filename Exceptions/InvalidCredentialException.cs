namespace PersonalKnowledgeHub.Exceptions
{
    public class InvalidCredentialException : Exception
    {
        public InvalidCredentialException(string message) : base(message) { }
    }
}
