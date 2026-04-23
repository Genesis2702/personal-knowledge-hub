namespace PersonalKnowledgeHub.DTOs.Requests;

public class ResetPasswordRequestDto
{
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
}