namespace PersonalKnowledgeHub.Entities;

public class RolePermission
{
    public required Role Role { get; set; }
    public required Permission Permission { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
}