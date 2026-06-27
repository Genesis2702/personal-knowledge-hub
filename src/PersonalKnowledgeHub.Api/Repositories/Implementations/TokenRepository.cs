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

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenForUpdateAsync(string token, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens.FromSqlInterpolated($@"
                SELECT * 
                FROM ""RefreshTokens""
                WHERE ""Token"" = {token}
                FOR UPDATE
            ").SingleOrDefaultAsync(cancellationToken);
        }

        public async Task RevokeRefreshTokenAsync(string token, int? replacedId, CancellationToken cancellationToken)
        {
            RefreshToken refreshToken = (await _dbContext.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken))!;
            refreshToken.Revoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            if (replacedId != null)
            {
                refreshToken.ReplacedByTokenId = replacedId;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeRefreshTokensByFamilyAsync(Guid familyId, int? replacedId, CancellationToken cancellationToken)
        {
            List<RefreshToken> refreshTokens = await _dbContext.RefreshTokens.Where(rt => rt.FamilyId == familyId).ToListAsync(cancellationToken);
            foreach (RefreshToken refreshToken in refreshTokens)
            {
                refreshToken.Revoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                if (replacedId != null)
                {
                    refreshToken.ReplacedByTokenId = replacedId;
                }
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeRefreshTokensByUserAsync(int userId, CancellationToken cancellationToken)
        {
            List<RefreshToken> refreshTokens = await _dbContext.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync(cancellationToken);
            foreach (RefreshToken refreshToken in refreshTokens)
            {
                refreshToken.Revoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task CleanUpRefreshTokenAsync(CancellationToken cancellationToken)
        {
            var revokedTime = DateTime.UtcNow.AddDays(-30);
            await _dbContext.RefreshTokens.Where(rt => rt.Revoked && rt.RevokedAt != null && rt.RevokedAt < revokedTime).ExecuteDeleteAsync(cancellationToken);
        }
    }
}
