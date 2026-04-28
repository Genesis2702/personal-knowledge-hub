using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class ResetPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    [StringLength(64, MinimumLength = 8)]
    public required string NewPassword { get; set; }
}