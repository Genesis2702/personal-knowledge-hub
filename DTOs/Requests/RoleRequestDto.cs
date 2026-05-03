using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class RoleRequestDto
{
    [Required]
    public required string Name { get; set; }
}