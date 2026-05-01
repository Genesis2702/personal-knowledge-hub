namespace PersonalKnowledgeHub.Entities;

public class VerificationToken
{
    public int Id { get; set; }
    public required string TokenHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}