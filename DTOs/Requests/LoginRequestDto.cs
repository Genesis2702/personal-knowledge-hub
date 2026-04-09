using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [MinLength(8)]
        public required string Password { get; set; }
    }
}
