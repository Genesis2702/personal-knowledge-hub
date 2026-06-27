namespace PersonalKnowledgeHub.Entities;

public class Permission
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}