using Microsoft.EntityFrameworkCore;
using PersonalKnowledgeHub.Data;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;

namespace PersonalKnowledgeHub.Repositories.Implementations;

public class VerificationTokenRepository : IVerificationTokenRepository
{
    private readonly AppDbContext _dbContext;

    public VerificationTokenRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;  
    }

    public async Task<VerificationToken?> GetVerificationTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbContext.VerificationTokens.SingleOrDefaultAsync(verificationToken =>
            verificationToken.Token == token, cancellationToken);
    }

    public async Task AddVerificationTokenAsync(VerificationToken verificationToken, CancellationToken cancellationToken)
    {
        await _dbContext.VerificationTokens.AddAsync(verificationToken, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ValidateVerificationTokenAsync(VerificationToken verificationToken, CancellationToken cancellationToken)
    {
        verificationToken.ExpiresAt = DateTime.UtcNow;
        verificationToken.VerifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task CleanUpVerificationTokenAsync(CancellationToken cancellationToken)
    {
        DateTime expiredTime = DateTime.UtcNow.AddHours(-24);
        await _dbContext.VerificationTokens.Where(verificationToken => verificationToken.ExpiresAt < expiredTime)
            .ExecuteDeleteAsync(cancellationToken);
    }
}