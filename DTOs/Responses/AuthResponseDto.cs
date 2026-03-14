namespace PersonalKnowledgeHub.DTOs.Responses
{
    public class AuthResponseDto
    {
        public required string RefreshToken { get; set; }
        public required string AccessToken { get; set; }
    }
}
