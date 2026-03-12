using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITokenService
    {
        public RefreshToken GenerateRefreshToken(User user);
        public string GenerateAccessToken(User user);
        public Task<bool> RevokeRefreshToken(string token);
        public Task<bool> ValidateRefreshToken(string token);
    }
}
