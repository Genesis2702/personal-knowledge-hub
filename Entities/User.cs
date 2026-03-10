namespace PersonalKnowledgeHub.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public DateOnly CreatedAt { get; set; }
    }
}
