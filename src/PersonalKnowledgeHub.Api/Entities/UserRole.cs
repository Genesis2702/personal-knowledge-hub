namespace PersonalKnowledgeHub.Entities;

public class UserRole
{
    public required User User { get; set; }
    public required Role Role { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
}