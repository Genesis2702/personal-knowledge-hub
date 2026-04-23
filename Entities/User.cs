namespace PersonalKnowledgeHub.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? BannedAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
