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

    public async Task<VerificationToken?> GetVerificationTokenAsync(string hashedToken)
    {
        return await _dbContext.VerificationTokens.SingleOrDefaultAsync(verificationToken =>
            verificationToken.TokenHash == hashedToken);
    }

    public async Task AddVerificationTokenAsync(VerificationToken verificationToken)
    {
        await _dbContext.VerificationTokens.AddAsync(verificationToken);
        await _dbContext.SaveChangesAsync();
    }

    public async Task ValidateVerificationTokenAsync(VerificationToken verificationToken)
    {
        verificationToken.ExpiresAt = DateTime.UtcNow;
        verificationToken.VerifiedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }

    public async Task CleanUpVerificationTokenAsync()
    {
        DateTime expiredTime = DateTime.UtcNow.AddHours(-24);
        await _dbContext.VerificationTokens.Where(verificationToken => verificationToken.ExpiresAt < expiredTime)
            .ExecuteDeleteAsync();
    }
}