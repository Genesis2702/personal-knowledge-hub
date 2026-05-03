using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateRefreshToken(int userId, Guid familyId);
        public Task<string> GenerateAccessToken(int userId);
        public Task RevokeRefreshToken(string token, int? replacedId);
        public Task<RefreshToken> ValidateRefreshToken(string rawToken);
        public Task<RefreshToken> GetRefreshToken(string rawToken);
    }
}
