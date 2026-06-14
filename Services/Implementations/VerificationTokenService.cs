using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class VerificationTokenService : IVerificationTokenService
{
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly ILogger<VerificationTokenService> _logger;

    public VerificationTokenService(IVerificationTokenRepository verificationTokenRepository, ILogger<VerificationTokenService> logger)
    {
        _verificationTokenRepository = verificationTokenRepository;
        _logger = logger;
    }

    public async Task<string> GenerateVerificationToken(int userId, CancellationToken cancellationToken)
    {
        string rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashedToken = SHA256.HashData(binaryToken);
        VerificationToken verificationToken = new VerificationToken
        {
            Token = Convert.ToHexString(hashedToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            VerifiedAt = null,
            UserId = userId
        };
        await _verificationTokenRepository.AddVerificationTokenAsync(verificationToken, cancellationToken);
        return rawToken;
    }

    public async Task ValidateVerificationToken(string rawToken, int userId, CancellationToken cancellationToken)
    {
        byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashedToken = SHA256.HashData(binaryToken);
        string token = Convert.ToHexString(hashedToken);
        VerificationToken? verificationToken = await _verificationTokenRepository.GetVerificationTokenAsync(token, cancellationToken);
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
        await _verificationTokenRepository.ValidateVerificationTokenAsync(verificationToken, cancellationToken);
    }

    public async Task<int> ValidatePasswordResetToken(string rawToken, CancellationToken cancellationToken)
    {
        byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
        byte[] hashedToken = SHA256.HashData(binaryToken);
        string token = Convert.ToHexString(hashedToken);
        VerificationToken? verificationToken = await _verificationTokenRepository.GetVerificationTokenAsync(token, cancellationToken);
        if (verificationToken == null)
        {
            throw new NotFoundException("Verification token not found");
        }
        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedException("Verification token expired");
        }
        if (verificationToken.VerifiedAt != null)
        {
            throw new ConflictException("Verification token already used");
        }
        await _verificationTokenRepository.ValidateVerificationTokenAsync(verificationToken, cancellationToken);
        return verificationToken.UserId;
    }

    public async Task CleanUpVerificationTokens(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verification tokens cleaning up started");
        await _verificationTokenRepository.CleanUpVerificationTokenAsync(cancellationToken);
        _logger.LogInformation("Verification tokens cleaned up successfully");
    }
}