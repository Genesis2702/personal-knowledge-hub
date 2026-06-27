using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateRefreshToken(int userId, Guid familyId, CancellationToken cancellationToken);
        public Task<string> GenerateAccessToken(int userId, CancellationToken cancellationToken);
        public Task RevokeRefreshToken(string token, int? replacedId, CancellationToken cancellationToken);
        public Task<RefreshToken> ValidateRefreshToken(string rawToken, CancellationToken cancellationToken);
        public Task<RefreshToken> GetRefreshToken(string rawToken, CancellationToken cancellationToken);
        public Task CleanUpRefreshTokens(CancellationToken cancellationToken);
    }
}
