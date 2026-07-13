using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;

namespace PersonalKnowledgeHub.UnitTests;

public class VerificationTokenServiceTests
{
    private readonly Mock<IVerificationTokenRepository> _verificationTokenRepository;
    private readonly IVerificationTokenService _verificationTokenService;

    public VerificationTokenServiceTests()
    {
        _verificationTokenRepository = new Mock<IVerificationTokenRepository>();
        _verificationTokenService = new VerificationTokenService(_verificationTokenRepository.Object,
            NullLogger<VerificationTokenService>.Instance);
    }
    
    [Fact]
    public async Task GenerateVerificationToken_WhenCalled_ReturnsRawToken()
    {
        int userId = 1;
        VerificationToken verificationToken = null;
        
        _verificationTokenRepository.Setup(x => x.AddVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()))
            .Callback<VerificationToken, CancellationToken>((token, _) => verificationToken = token)
            .Returns(Task.CompletedTask);
        
        var result = await _verificationTokenService.GenerateVerificationToken(userId, CancellationToken.None);
        
        Assert.Equal(verificationToken!.Token, result);
        Assert.Equal(userId, verificationToken.UserId);
        
        _verificationTokenRepository.Verify(x => x.AddVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task ValidateVerificationToken_WhenTokenExists_ValidatesToken()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken verificationToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            VerifiedAt = null,
            UserId = userId
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationToken);
        _verificationTokenRepository.Setup(x => x.ValidateVerificationTokenAsync(verificationToken, It.IsAny<CancellationToken>()))
            .Callback<VerificationToken, CancellationToken>((t, _) =>
            {
                t.ExpiresAt = DateTime.UtcNow;
                t.VerifiedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);
        
        await _verificationTokenService.ValidateVerificationToken(token, userId, CancellationToken.None);
        
        Assert.NotNull(verificationToken.VerifiedAt);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(verificationToken, It.IsAny<CancellationToken>()), Times.Once);       
        
    }

    [Fact]
    public async Task ValidateVerificationToken_WhenTokenDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        string token = "test token";
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VerificationToken?)null);

        Func<Task> result = () =>
            _verificationTokenService.ValidateVerificationToken(token, userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);      
    }
    
    [Fact]
    public async Task ValidateVerificationToken_WhenTokenExpired_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken verificationToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            VerifiedAt = null,
            UserId = userId
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationToken);
        
        Func<Task> result = () =>
            _verificationTokenService.ValidateVerificationToken(token, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);     
    }
    
    [Fact]
    public async Task ValidateVerificationToken_WhenUserDoesNotOwnToken_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken verificationToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            VerifiedAt = null,
            UserId = 20
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationToken);
        
        Func<Task> result = () =>
            _verificationTokenService.ValidateVerificationToken(token, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);    
    }
    
    [Fact]
    public async Task ValidateVerificationToken_WhenTokenAlreadyVerified_ThrowsConflictException()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken verificationToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            VerifiedAt = DateTime.UtcNow,
            UserId = userId
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationToken);
        
        Func<Task> result = () =>
            _verificationTokenService.ValidateVerificationToken(token, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);   
    }

    [Fact]
    public async Task ValidatePasswordResetToken_WhenTokenExists_ReturnsToken()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken passwordResetToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            VerifiedAt = null,
            UserId = userId,
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordResetToken);
        _verificationTokenRepository.Setup(x => x.ValidateVerificationTokenAsync(passwordResetToken, It.IsAny<CancellationToken>()))
            .Callback<VerificationToken, CancellationToken>((t, _) =>
            {
                t.ExpiresAt = DateTime.UtcNow;
                t.VerifiedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);
        
        var result = await _verificationTokenService.ValidatePasswordResetToken(token, CancellationToken.None);
        
        Assert.Equal(userId, result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(passwordResetToken, It.IsAny<CancellationToken>()), Times.Once);      
    }

    [Fact]
    public async Task ValidatePasswordResetToken_WhenTokenDoesNotExist_ThrowsNotFoundException()
    {
        string token = "test token";
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((VerificationToken?)null);
        
        Func<Task> result = () => _verificationTokenService.ValidatePasswordResetToken(token, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);     
    }
    
    [Fact]
    public async Task ValidatePasswordResetToken_WhenTokenExpired_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken passwordResetToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            VerifiedAt = null,
            UserId = userId,
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordResetToken);
        
        Func<Task> result = () => _verificationTokenService.ValidatePasswordResetToken(token, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);    
    }
    
    [Fact]
    public async Task ValidatePasswordResetToken_WhenTokenAlreadyVerified_ThrowsConflictException()
    {
        int userId = 1;
        string token = "test token";

        VerificationToken passwordResetToken = new VerificationToken
        {
            Id = 1,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            VerifiedAt = DateTime.UtcNow,
            UserId = userId,
        };
        
        _verificationTokenRepository.Setup(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordResetToken);
        
        Func<Task> result = () => _verificationTokenService.ValidatePasswordResetToken(token, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _verificationTokenRepository.Verify(x => x.GetVerificationTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);      
        _verificationTokenRepository.Verify(x => x.ValidateVerificationTokenAsync(It.IsAny<VerificationToken>(), It.IsAny<CancellationToken>()), Times.Never);  
    }

    [Fact]
    public async Task CleanUpVerificationTokens_WhenCalled_CallsRepository()
    {
        await _verificationTokenService.CleanUpVerificationTokens(CancellationToken.None);
        
        _verificationTokenRepository.Verify(x => x.CleanUpVerificationTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}