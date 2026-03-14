using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        public Task<RefreshToken?> GetRefreshTokenAsync(string token);
        public Task RevokeRefreshTokenAsync(string token);
        public Task CleanUpRefreshTokenAsync();
    }
}
