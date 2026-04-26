using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        public Task<RefreshToken?> GetRefreshTokenAsync(string token);
        public Task<RefreshToken?> GetRefreshTokenForUpdateAsync(string token);
        public Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken);
        public Task RevokeRefreshTokenAsync(string token, int? replacedId);
        public Task RevokeRefreshTokensByFamilyAsync(Guid familyId, int? replacedId);
        public Task RevokeRefreshTokensByUserAsync(int userId);
        public Task CleanUpRefreshTokenAsync();
    }
}
