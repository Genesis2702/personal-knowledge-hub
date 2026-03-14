using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _dbContext;

        public TokenRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            RefreshToken? refreshToken = await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token);
            if (refreshToken == null) { return false; }
            refreshToken.Revoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task CleanUpRefreshTokenAsync()
        {
            var revokedTime = DateTime.UtcNow.AddDays(-30);
            await _dbContext.RefreshTokens.Where(rt => rt.Revoked && rt.RevokedAt != null && rt.RevokedAt < revokedTime).ExecuteDeleteAsync();
        }
    }
}
