using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class RegisterRequestDto
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [MinLength(8)]
        public required string Password { get; set; }
    }
}
