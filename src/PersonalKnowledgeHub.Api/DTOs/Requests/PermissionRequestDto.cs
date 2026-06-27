using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class PermissionRequestDto
{
    [Required]
    public required string Name { get; set; }
}