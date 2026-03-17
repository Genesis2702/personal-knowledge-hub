using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITokenService
    {
        public Task<RefreshToken> GenerateRefreshToken(int userId);
        public Task<string> GenerateAccessToken(int userId);
        public Task RevokeRefreshToken(string token);
        public Task<RefreshToken> ValidateRefreshToken(string token);
        public Task<RefreshToken> GetRefreshToken(string token);
    }
}
