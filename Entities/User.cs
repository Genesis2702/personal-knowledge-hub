namespace PersonalKnowledgeHub.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
