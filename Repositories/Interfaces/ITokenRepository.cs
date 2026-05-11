using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        public Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken);
        public Task<RefreshToken?> GetRefreshTokenForUpdateAsync(string token, CancellationToken cancellationToken);
        public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        public Task RevokeRefreshTokenAsync(string token, int? replacedId, CancellationToken cancellationToken);
        public Task RevokeRefreshTokensByFamilyAsync(Guid familyId, int? replacedId, CancellationToken cancellationToken);
        public Task RevokeRefreshTokensByUserAsync(int userId, CancellationToken cancellationToken);
        public Task CleanUpRefreshTokenAsync(CancellationToken cancellationToken);
    }
}
