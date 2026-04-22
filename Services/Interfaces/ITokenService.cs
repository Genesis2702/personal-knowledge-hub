using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITokenService
    {
        public Task<RefreshToken> GenerateRefreshToken(int userId, Guid familyId);
        public Task<string> GenerateAccessToken(int userId);
        public Task RevokeRefreshToken(string token, int? replacedId);
        public Task<RefreshToken> ValidateRefreshToken(string token);
        public Task<RefreshToken> GetRefreshToken(string token);
    }
}
