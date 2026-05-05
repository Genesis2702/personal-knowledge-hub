using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class UserUpdateRequestDto
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public required string UserName { get; set; }
}