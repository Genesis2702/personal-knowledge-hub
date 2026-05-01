using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IVerificationTokenRepository
{
    public Task<VerificationToken?> GetVerificationTokenAsync(string hashedToken);
    public Task AddVerificationTokenAsync(VerificationToken verificationToken);
    public Task ValidateVerificationTokenAsync(VerificationToken verificationToken, User user);
    public Task CleanUpVerificationTokenAsync();
}