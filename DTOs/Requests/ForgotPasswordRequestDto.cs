using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}