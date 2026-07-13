using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class TokenServiceTests
{
    private readonly Mock<ITokenRepository> _tokenRepository;
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IConfiguration> _configuration;
    private readonly ITokenService _tokenService;

    public TokenServiceTests()
    {
        _tokenRepository = new Mock<ITokenRepository>();
        _userRepository = new Mock<IUserRepository>();
        _configuration = new Mock<IConfiguration>();
        _tokenService = new TokenService(_tokenRepository.Object, _userRepository.Object, _configuration.Object,
            NullLogger<TokenService>.Instance);
    }

    [Fact]
    public async Task GenerateRefreshToken_WhenCalled_ReturnsRawToken()
    {
        int userId = 1;
        Guid familyId = Guid.NewGuid();
        RefreshToken refreshToken = null;
        
        _tokenRepository.Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((token, _) => refreshToken = token)
            .Returns(Task.CompletedTask);
        
        var result = await _tokenService.GenerateRefreshToken(userId, familyId, CancellationToken.None);

        string expectedToken = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(result)));
        
        Assert.Equal(expectedToken, refreshToken!.Token);
        Assert.Equal(userId, refreshToken.UserId);
        Assert.Equal(familyId, refreshToken.FamilyId);
        Assert.False(refreshToken.Revoked);
        Assert.Null(refreshToken.RevokedAt);
        Assert.Null(refreshToken.ReplacedByTokenId);
        
        _tokenRepository.Verify(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateAccessToken_WhenCalled_ReturnsRawToken()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
            Status = UserStatus.Active
        };
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        var result = await _tokenService.GenerateAccessToken(userId, CancellationToken.None);
        
        JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(result);
        
        Assert.Equal(userId.ToString(), token.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(userEmail, token.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(user.Status.ToString(), token.Claims.First(c => c.Type == "status").Value);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task RevokeRefreshToken_WhenCalled_RevokesRefreshToken()
    {
        string token = "test token";
        int replacerId = 2;
        
        RefreshToken refreshToken = new RefreshToken
        {
            Id = 1,
            Token = token,
            Revoked = false,
            RevokedAt = null,
            ReplacedByTokenId = null
        };
        
        _tokenRepository.Setup(x => x.RevokeRefreshTokenAsync(token, replacerId, It.IsAny<CancellationToken>()))
            .Callback<string, int?, CancellationToken>((t, r, _) =>
            {
                refreshToken.Revoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.ReplacedByTokenId = r;
            })
            .Returns(Task.CompletedTask);
        
        await _tokenService.RevokeRefreshToken(token, replacerId, CancellationToken.None);
        
        Assert.True(refreshToken.Revoked);
        Assert.NotNull(refreshToken.RevokedAt);
        Assert.Equal(replacerId, refreshToken.ReplacedByTokenId);
        
        _tokenRepository.Verify(x => x.RevokeRefreshTokenAsync(token, replacerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateRefreshToken_WhenRefreshTokenExists_ReturnsRefreshToken()
    {
        string token = "test token";

        RefreshToken refreshToken = new RefreshToken
        {
            Id = 1,
            Token = token,
            Revoked = false,
            RevokedAt = null,
            ReplacedByTokenId = null,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        
        _tokenRepository.Setup(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        
        var result = await _tokenService.ValidateRefreshToken(token, CancellationToken.None);
        
        Assert.Equal(refreshToken, result);
        
        _tokenRepository.Verify(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task ValidateRefreshToken_WhenRefreshTokenDoesNotExist_ThrowsNotFoundException()
    {
        string token = "test token";
        
        _tokenRepository.Setup(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        Func<Task> result = () => _tokenService.ValidateRefreshToken(token, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _tokenRepository.Verify(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);   
    }
    
    [Fact]
    public async Task ValidateRefreshToken_WhenRefreshTokenAlreadyRevoked_ThrowsUnauthorizedException()
    {
        string token = "test token";

        RefreshToken refreshToken = new RefreshToken
        {
            Id = 1,
            Token = token,
            Revoked = true,
            RevokedAt = DateTime.UtcNow.AddDays(-1),
            ReplacedByTokenId = null,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
        
        _tokenRepository.Setup(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        
        Func<Task> result = () => _tokenService.ValidateRefreshToken(token, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _tokenRepository.Verify(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);  
    }
    
    [Fact]
    public async Task ValidateRefreshToken_WhenRefreshTokenExpired_ThrowsUnauthorizedException()
    {
        string token = "test token";

        RefreshToken refreshToken = new RefreshToken
        {
            Id = 1,
            Token = token,
            Revoked = false,
            RevokedAt = null,
            ReplacedByTokenId = null,
            ExpiresAt = DateTime.UtcNow.AddDays(-1)
        };
        
        _tokenRepository.Setup(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        
        Func<Task> result = () => _tokenService.ValidateRefreshToken(token, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _tokenRepository.Verify(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once); 
    }
    
    [Fact]
    public async Task GetRefreshToken_WhenRefreshTokenExists_ReturnsRefreshToken()
    {
        string token = "test token";

        RefreshToken refreshToken = new RefreshToken
        {
            Id = 1,
            Token = token
        };
        
        _tokenRepository.Setup(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        
        var result = await _tokenService.GetRefreshToken(token, CancellationToken.None);
        
        Assert.Equal(refreshToken, result);
        
        _tokenRepository.Verify(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetRefreshToken_WhenRefreshTokenDoesNotExist_ThrowsNotFoundException()
    {
        string token = "test token";
        
        _tokenRepository.Setup(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);
        
        Func<Task> result = () => _tokenService.GetRefreshToken(token, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);      
        
        _tokenRepository.Verify(x => x.GetRefreshTokenAsync(token, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CleanUpRefreshTokens_WhenCalled_CallsRepository()
    {
        await _tokenService.CleanUpRefreshTokens(CancellationToken.None);
        
        _tokenRepository.Verify(x => x.CleanUpRefreshTokenAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}