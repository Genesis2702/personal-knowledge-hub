using System.ComponentModel.DataAnnotations;

namespace PersonalKnowledgeHub.DTOs.Requests
{
    public class RefreshRequestDto
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
