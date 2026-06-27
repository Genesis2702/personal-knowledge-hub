using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IVerificationTokenService
{
    public Task<string> GenerateVerificationToken(int userId, CancellationToken cancellationToken);
    public Task ValidateVerificationToken(string rawToken, int userId, CancellationToken cancellationToken);
    public Task<int> ValidatePasswordResetToken(string rawToken, CancellationToken cancellationToken);
    public Task CleanUpVerificationTokens(CancellationToken cancellationToken);
}