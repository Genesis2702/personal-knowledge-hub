using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class LogoutRequestDto
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
