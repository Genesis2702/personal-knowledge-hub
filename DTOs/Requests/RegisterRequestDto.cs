using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public required string UserName { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [StringLength(64, MinimumLength = 8)]
        public required string Password { get; set; }
    }
}
