using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces;

public interface IVerificationTokenService
{
    public Task<string> GenerateVerificationToken(int userId);
    public Task ValidateVerificationToken(string rawToken, int userId);
    public Task<int> ValidatePasswordResetToken(string rawToken);
}