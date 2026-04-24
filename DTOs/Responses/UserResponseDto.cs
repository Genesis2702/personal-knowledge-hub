namespace PersonalKnowledgeHub.DTOs.Responses;

public class UserResponseDto
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsBanned { get; set; }
    public DateTime? BannedAt { get; set; }
    public ICollection<string> Resources { get; set; } = new List<string>();
}