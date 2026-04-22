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

        public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken);
            await _dbContext.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenForUpdateAsync(string token)
        {
            return await _dbContext.RefreshTokens.FromSqlInterpolated($@"
                SELECT * 
                FROM ""RefreshTokens""
                WHERE ""Token"" = {token}
                FOR UPDATE
            ").SingleOrDefaultAsync();
        }

        public async Task RevokeRefreshTokenAsync(string token, int? replacedId)
        {
            RefreshToken refreshToken = (await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token))!;
            refreshToken.Revoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            if (replacedId != null)
            {
                refreshToken.ReplacedByTokenId = replacedId;
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task RevokeAllRefreshTokensAsync(Guid familyId, int? replacedId)
        {
            List<RefreshToken> refreshTokens = await _dbContext.RefreshTokens.Where(rt => rt.FamilyId == familyId).ToListAsync();
            foreach (RefreshToken refreshToken in refreshTokens)
            {
                refreshToken.Revoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                if (replacedId != null)
                {
                    refreshToken.ReplacedByTokenId = replacedId;
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task CleanUpRefreshTokenAsync()
        {
            var revokedTime = DateTime.UtcNow.AddDays(-30);
            await _dbContext.RefreshTokens.Where(rt => rt.Revoked && rt.RevokedAt != null && rt.RevokedAt < revokedTime).ExecuteDeleteAsync();
        }
    }
}
