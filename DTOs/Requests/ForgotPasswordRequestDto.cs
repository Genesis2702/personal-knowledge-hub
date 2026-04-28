using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class ForgotPasswordRequestDto
{
    [Required]
    [StringLength(64, MinimumLength = 8)]
    public required string NewPassword { get; set; }
}