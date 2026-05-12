using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Repositories.Interfaces;

public interface IVerificationTokenRepository
{
    public Task<VerificationToken?> GetVerificationTokenAsync(string token, CancellationToken cancellationToken);
    public Task AddVerificationTokenAsync(VerificationToken verificationToken, CancellationToken cancellationToken);
    public Task ValidateVerificationTokenAsync(VerificationToken verificationToken, CancellationToken cancellationToken);
    public Task CleanUpVerificationTokenAsync(CancellationToken cancellationToken);
}