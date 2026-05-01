using System.Security.Cryptography;
using System.Text;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class VerificationTokenService : IVerificationTokenService
{
    private readonly IVerificationTokenRepository _verificationTokenRepository;

    public VerificationTokenService(IVerificationTokenRepository verificationTokenRepository)
    {
        _verificationTokenRepository = verificationTokenRepository;
    }

    public async Task<string> GenerateVerificationToken(int userId)
    {
        string rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashedToken = SHA256.HashData(binaryToken);
        VerificationToken verificationToken = new VerificationToken
        {
            TokenHash = Convert.ToHexString(hashedToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            VerifiedAt = null,
            UserId = userId
        };
        await _verificationTokenRepository.AddVerificationTokenAsync(verificationToken);
        return rawToken;
    }

    public async Task ValidateVerificationToken(string rawToken, int userId)
    {
        byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashedToken = SHA256.HashData(binaryToken);
        string token = Convert.ToHexString(hashedToken);
        VerificationToken? verificationToken = await _verificationTokenRepository.GetVerificationTokenAsync(token);
        if (verificationToken == null)
        {
            throw new NotFoundException("Verification token not found");
        }
        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Verification token expired");
        }
        if (verificationToken.UserId != userId)
        {
            throw new UnauthorizedException("Verification token belongs to another user");
        }
        if (verificationToken.VerifiedAt != null)
        {
            throw new ConflictException("Verification token already used");
        }
        await _verificationTokenRepository.ValidateVerificationTokenAsync(verificationToken);
    }
}