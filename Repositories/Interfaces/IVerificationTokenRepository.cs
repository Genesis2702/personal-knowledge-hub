using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IVerificationTokenRepository
{
    public Task<VerificationToken?> GetVerificationTokenAsync(string token);
    public Task AddVerificationTokenAsync(VerificationToken verificationToken);
    public Task ValidateVerificationTokenAsync(VerificationToken verificationToken);
    public Task CleanUpVerificationTokenAsync();
}