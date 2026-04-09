using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Responses
{
    public class AuthResponseDto
    {
        [Required]
        public required string RefreshToken { get; set; }
        [Required]
        public required string AccessToken { get; set; }
    }
}
