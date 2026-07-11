using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Observability;
using PersonalKnowledgeHub.Services.Implementations;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.EntityFrameworkCore.Storage;
using PersonalKnowledgeHub.Exceptions;

namespace PersonalKnowledgeHub.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<IUnitOfWorkRepository> _unitOfWorkRepository;
    private readonly Mock<IMailFactoryService> _mailFactoryService;
    private readonly Mock<IVerificationTokenService> _verificationTokenService;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClient;
    private readonly Mock<IDbContextTransaction> _transaction = new();
    private readonly Mock<AppMetrics> _metrics;
    private readonly IAuthService _authService;
    
    public AuthServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _tokenService = new Mock<ITokenService>();
        _unitOfWorkRepository = new Mock<IUnitOfWorkRepository>();
        _mailFactoryService = new Mock<IMailFactoryService>();
        _verificationTokenService = new Mock<IVerificationTokenService>();
        _backgroundJobClient = new Mock<IBackgroundJobClient>();
        _metrics = new Mock<AppMetrics>();
        _authService = new AuthService(_userRepository.Object, _tokenService.Object, _unitOfWorkRepository.Object,
            _mailFactoryService.Object,
            _verificationTokenService.Object, _backgroundJobClient.Object, NullLogger<AuthService>.Instance,
            _metrics.Object);
    }

    [Fact]
    public async Task RegisterUser_WhenUserDoesNotExist_ReturnsRegisteredUser()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "email@gmail.com";
        string userPassword = "test password";
        string refreshToken = "test refresh token";
        string accessToken = "test access token";
        string verificationToken = "test verification token";

        RegisterRequestDto registerRequest = new RegisterRequestDto
        {
            UserName = userName,
            Email = userEmail,
            Password = userPassword
        };

        MailData mailData = new MailData
        {
            EmailToId = registerRequest.Email,
            EmailToName = registerRequest.UserName,
            EmailSubject = "verification",
            EmailBody = "test"
        };

        _userRepository.Setup(x => x.IsEmailExistAsync(registerRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepository.Setup(x => x.AddUserAsync(It.Is<User>(u => 
            u.UserName == registerRequest.UserName && u.Email == registerRequest.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User addedUser, CancellationToken _) =>
            {
                addedUser.Id = userId;
                return addedUser;
            });
        _tokenService.Setup(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _tokenService.Setup(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);
        _verificationTokenService.Setup(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationToken);
        _mailFactoryService.Setup(x => x.CreateVerificationMail(It.Is<User>(u =>
            u.Email == registerRequest.Email && u.UserName == registerRequest.UserName), It.IsAny<string>()))
            .Returns(mailData);
        
        var result = await _authService.RegisterUser(registerRequest, CancellationToken.None);
        
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(accessToken, result.AccessToken);
        
        _userRepository.Verify(x => x.IsEmailExistAsync(registerRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.AddUserAsync(It.Is<User>(u => 
            u.UserName == registerRequest.UserName && u.Email == registerRequest.Email), It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mailFactoryService.Verify(x => x.CreateVerificationMail(It.Is<User>(u =>
            u.Email == registerRequest.Email && u.UserName == registerRequest.UserName), It.IsAny<string>()), Times.Once);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterUser_WhenEmailIsNotValid_ThrowsValidationException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";

        RegisterRequestDto registerRequest = new RegisterRequestDto
        {
            UserName = userName,
            Email = userEmail,
            Password = userPassword
        };
        
        Func<Task> result = () => _authService.RegisterUser(registerRequest, CancellationToken.None);

        await Assert.ThrowsAsync<ValidationException>(result);
        
        _userRepository.Verify(x => x.IsEmailExistAsync(registerRequest.Email, It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(x => x.AddUserAsync(It.Is<User>(u => 
            u.UserName == registerRequest.UserName && u.Email == registerRequest.Email), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Never);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Never);
        _mailFactoryService.Verify(x => x.CreateVerificationMail(It.Is<User>(u =>
            u.Email == registerRequest.Email && u.UserName == registerRequest.UserName), It.IsAny<string>()), Times.Never);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterUser_WhenUserAlreadyExists_ThrowsConflictException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "email@gmail.com";
        string userPassword = "test password";

        RegisterRequestDto registerRequest = new RegisterRequestDto
        {
            UserName = userName,
            Email = userEmail,
            Password = userPassword
        };
        
        _userRepository.Setup(x => x.IsEmailExistAsync(registerRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        Func<Task> result = () => _authService.RegisterUser(registerRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _userRepository.Verify(x => x.IsEmailExistAsync(registerRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.AddUserAsync(It.Is<User>(u => 
            u.UserName == registerRequest.UserName && u.Email == registerRequest.Email), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Never);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Never);
        _mailFactoryService.Verify(x => x.CreateVerificationMail(It.Is<User>(u =>
            u.Email == registerRequest.Email && u.UserName == registerRequest.UserName), It.IsAny<string>()), Times.Never);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Never);
    }

    [Fact]
    public async Task AuthenticateUser_WhenUserExists_ReturnsAuthenticatedUser()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "email@gmail.com";
        string userPassword = "test password";
        string refreshToken = "test refresh token";
        string accessToken = "test access token";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
            FailedLoginAttempts = 0,
            LockedUntil = null
        };

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Email = userEmail,
            Password = userPassword
        };

        _userRepository.Setup(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.ResetFailedLoginAttemptsAsync(userId, It.IsAny<CancellationToken>()))
            .Callback<int, CancellationToken>((id, _) =>
            {
                user.FailedLoginAttempts = 0;
                user.LockedUntil = null;
            })
            .ReturnsAsync(1);
        _tokenService.Setup(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _tokenService.Setup(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);
        
        var result = await _authService.AuthenticateUser(loginRequest, CancellationToken.None);
        
        Assert.Equal(refreshToken, result.RefreshToken);
        Assert.Equal(accessToken, result.AccessToken);
        
        _userRepository.Verify(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetFailedLoginAttemptsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateUser_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        string userEmail = "email@gmail.com";
        string userPassword = "test password";

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Email = userEmail,
            Password = userPassword
        };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _authService.AuthenticateUser(loginRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetFailedLoginAttemptsAsync(userId, It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateUser_WhenUserIsLocked_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "email@gmail.com";
        string userPassword = "test password";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
            FailedLoginAttempts = 0,
            LockedUntil = DateTime.UtcNow.AddMinutes(2)
        };

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Email = userEmail,
            Password = userPassword
        };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        Func<Task> result = () => _authService.AuthenticateUser(loginRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _userRepository.Verify(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetFailedLoginAttemptsAsync(userId, It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task AuthenticateUser_WhenPasswordIsInvalid_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "email@gmail.com";
        string userCorrectPassword = "correct password";
        string userIncorrectPassword = "incorrect password";
        int failedLoginLimit = 5;
        int lockedMinutes = 2;

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userCorrectPassword,
            FailedLoginAttempts = 0,
            LockedUntil = null
        };

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Email = userEmail,
            Password = userIncorrectPassword
        };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.UpdateFailedLoginAttemptsAsync(userId, failedLoginLimit, lockedMinutes, It.IsAny<CancellationToken>()))
            .Callback<int, int, DateTime?, CancellationToken>((id, limit, lockedUntil, _) =>
            {
                user.FailedLoginAttempts++;
            })
            .ReturnsAsync(1);
        
        Func<Task> result = () => _authService.AuthenticateUser(loginRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _userRepository.Verify(x => x.GetUserByEmailAsync(loginRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.UpdateFailedLoginAttemptsAsync(userId, failedLoginLimit, lockedMinutes, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetFailedLoginAttemptsAsync(userId, It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateRefreshToken(userId, Guid.NewGuid(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(userId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshUser_WhenRefreshTokenIsValid_ReturnsNewTokens()
    {
        string oldToken = "old refresh token";
        string newToken = "new refresh token";
        string accessToken = "access token";

        RefreshRequestDto refreshRequest = new RefreshRequestDto
        {
            RefreshToken = oldToken
        };

        RefreshToken oldRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = oldToken,
            Revoked = false,
            RevokedAt = null,
            ReplacedByTokenId = null
        };

        RefreshToken newRefreshToken = new RefreshToken
        {
            Id = 2,
            Token = newToken,
            Revoked = false,
            RevokedAt = null,
            ReplacedByTokenId = null
        };

        _unitOfWorkRepository.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);
        _tokenService.Setup(x => x.ValidateRefreshToken(oldToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldRefreshToken);
        _tokenService.Setup(x => x.GenerateRefreshToken(oldRefreshToken.UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newToken);
        _tokenService.Setup(x => x.GetRefreshToken(newToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newRefreshToken);
        _tokenService.Setup(x => x.RevokeRefreshToken(oldToken, newRefreshToken.Id, It.IsAny<CancellationToken>()))
            .Callback<string, int?, CancellationToken>((token, id, _) =>
            {
                oldRefreshToken.Revoked = true;
                oldRefreshToken.RevokedAt = DateTime.UtcNow;
                oldRefreshToken.ReplacedByTokenId = id;
            })
            .Returns(Task.CompletedTask);
        _tokenService.Setup(x => x.GenerateAccessToken(oldRefreshToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken);
        
        var result = await _authService.RefreshUser(refreshRequest, CancellationToken.None);
        
        Assert.Equal(newToken, result.RefreshToken);
        Assert.Equal(accessToken, result.AccessToken);
        
        _unitOfWorkRepository.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.ValidateRefreshToken(oldToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateRefreshToken(oldRefreshToken.UserId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GetRefreshToken(newToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.RevokeRefreshToken(oldToken, newRefreshToken.Id, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateAccessToken(oldRefreshToken.UserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshUser_WhenNotFoundExceptionOccurs_ThrowsNotFoundException()
    {
        string oldToken = "old refresh token";

        RefreshRequestDto refreshRequest = new RefreshRequestDto
        {
            RefreshToken = oldToken
        };

        _unitOfWorkRepository.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);
        _tokenService.Setup(x => x.ValidateRefreshToken(oldToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("refresh token not found"));
        
        Func<Task> result = () => _authService.RefreshUser(refreshRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _unitOfWorkRepository.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.ValidateRefreshToken(oldToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.RevokeRefreshToken(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task RefreshUser_WhenUnauthorizedExceptionOccurs_ThrowsUnauthorizedException()
    {
        string oldToken = "old refresh token";

        RefreshRequestDto refreshRequest = new RefreshRequestDto
        {
            RefreshToken = oldToken
        };

        _unitOfWorkRepository.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transaction.Object);
        _tokenService.Setup(x => x.ValidateRefreshToken(oldToken, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedException("refresh token is invalid"));
        
        Func<Task> result = () => _authService.RefreshUser(refreshRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);
        
        _unitOfWorkRepository.Verify(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.ValidateRefreshToken(oldToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.GenerateRefreshToken(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GetRefreshToken(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.RevokeRefreshToken(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenService.Verify(x => x.GenerateAccessToken(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutUser_WhenUserOwnsRefreshToken_RevokesRefreshToken()
    {
        int userId = 1;
        string refreshTokenString = "test refresh token";

        RefreshToken refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = userId,
            Revoked = false
        };

        LogoutRequestDto logoutRequest = new LogoutRequestDto
        {
            RefreshToken = refreshTokenString
        };
        
        _tokenService.Setup(x => x.GetRefreshToken(logoutRequest.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _tokenService.Setup(x => x.RevokeRefreshToken(logoutRequest.RefreshToken, null, It.IsAny<CancellationToken>()))
            .Callback<string, DateTime?, CancellationToken>((token, _, __) =>
            {
                refreshToken.Revoked = true;
            })
            .Returns(Task.CompletedTask);
        
        await _authService.LogoutUser(logoutRequest, userId, CancellationToken.None);
        
        Assert.True(refreshToken.Revoked);
        
        _tokenService.Verify(x => x.GetRefreshToken(logoutRequest.RefreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.RevokeRefreshToken(logoutRequest.RefreshToken, null, It.IsAny<CancellationToken>()), Times.Once);       
    }

    [Fact]
    public async Task LogoutUser_WhenUserDoesNotOwnRefreshToken_ThrowsForbiddenException()
    {
        int userId = 1;
        string refreshTokenString = "test refresh token";

        RefreshToken refreshToken = new RefreshToken
        {
            Token = refreshTokenString,
            UserId = 2,
            Revoked = false
        };

        LogoutRequestDto logoutRequest = new LogoutRequestDto
        {
            RefreshToken = refreshTokenString
        };
        
        _tokenService.Setup(x => x.GetRefreshToken(logoutRequest.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        
        Func<Task> result = () => _authService.LogoutUser(logoutRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ForbiddenException>(result);
        
        _tokenService.Verify(x => x.GetRefreshToken(logoutRequest.RefreshToken, It.IsAny<CancellationToken>()), Times.Once);
        _tokenService.Verify(x => x.RevokeRefreshToken(logoutRequest.RefreshToken, null, It.IsAny<CancellationToken>()), Times.Never);       
    }

    [Fact]
    public async Task ForgotPassword_WhenUserExists_SendsPasswordResetEmail()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string passwordResetToken = "test password reset token";

        User user = new User
        {
            Id = userId,
            UserName = userName, 
            Email = userEmail,
            PasswordHash = userPassword,
        };

        ForgotPasswordRequestDto forgotPasswordRequest = new ForgotPasswordRequestDto
        {
            Email = userEmail
        };
        
        MailData mailData = new MailData
        {
            EmailToId = userEmail,
            EmailToName = userName,
            EmailSubject = "password reset",
            EmailBody = "test"
        };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(forgotPasswordRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _verificationTokenService.Setup(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(passwordResetToken);
        _mailFactoryService.Setup(x => x.CreatePasswordResetMail(It.Is<User>(u =>
            u.Email == forgotPasswordRequest.Email && u.UserName == userName), It.IsAny<string>()))
            .Returns(mailData);
        
        await _authService.ForgotPassword(forgotPasswordRequest, CancellationToken.None);
        
        _userRepository.Verify(x => x.GetUserByEmailAsync(forgotPasswordRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mailFactoryService.Verify(x => x.CreatePasswordResetMail(It.Is<User>(u =>
            u.Email == forgotPasswordRequest.Email && u.UserName == userName), It.IsAny<string>()), Times.Once);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";

        ForgotPasswordRequestDto forgotPasswordRequest = new ForgotPasswordRequestDto
        {
            Email = userEmail
        };
        
        _userRepository.Setup(x => x.GetUserByEmailAsync(forgotPasswordRequest.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _authService.ForgotPassword(forgotPasswordRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);       
        
        _userRepository.Verify(x => x.GetUserByEmailAsync(forgotPasswordRequest.Email, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Never);
        _mailFactoryService.Verify(x => x.CreatePasswordResetMail(It.Is<User>(u =>
            u.Email == forgotPasswordRequest.Email && u.UserName == userName), It.IsAny<string>()), Times.Never);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WhenUserExists_UpdatesPassword()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string newPassword = "new password";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword
        };

        ResetPasswordRequestDto resetPasswordRequest = new ResetPasswordRequestDto
        {
            NewPassword = newPassword,
            ConfirmationPassword = newPassword
        };

        MailData mailData = new MailData
        {
            EmailToId = userEmail,
            EmailToName = userName,
            EmailSubject = "password reset",
            EmailBody = "test"
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.ResetPasswordAsync(userId, newPassword, It.IsAny<CancellationToken>()))
            .Callback<int, string, CancellationToken>((id, password, _) =>
                {
                    user.PasswordHash = password;
                })
            .ReturnsAsync(1);
        _mailFactoryService.Setup(x => x.CreatePasswordChangedMail(It.Is<User>(u =>
                u.Email == userEmail &&
                u.UserName == userName)))
            .Returns(mailData);
        
        await _authService.ResetPassword(resetPasswordRequest, userId, CancellationToken.None);
        
        Assert.Equal(newPassword, user.PasswordHash);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetPasswordAsync(userId, newPassword, It.IsAny<CancellationToken>()), Times.Once);
        _mailFactoryService.Verify(x => x.CreatePasswordChangedMail(It.Is<User>(u =>
            u.Email == userEmail &&
            u.UserName == userName)), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string newPassword = "new password";

        ResetPasswordRequestDto resetPasswordRequest = new ResetPasswordRequestDto
        {
            NewPassword = newPassword,
            ConfirmationPassword = newPassword
        };
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _authService.ResetPassword(resetPasswordRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);       
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetPasswordAsync(userId, newPassword, It.IsAny<CancellationToken>()), Times.Never);
        _mailFactoryService.Verify(
            x => x.CreatePasswordChangedMail(It.Is<User>(u => u.Email == userEmail && u.UserName == userName)),
            Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WhenPasswordDoesNotMatch_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string newPassword = "new password";
        string confirmationPassword = "wrong password";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword
        };

        ResetPasswordRequestDto resetPasswordRequest = new ResetPasswordRequestDto
        {
            NewPassword = newPassword,
            ConfirmationPassword = confirmationPassword
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        Func<Task> result = () => _authService.ResetPassword(resetPasswordRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);       
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetPasswordAsync(userId, newPassword, It.IsAny<CancellationToken>()), Times.Never);
        _mailFactoryService.Verify(x => x.CreatePasswordChangedMail(It.Is<User>(u =>
            u.Email == userEmail &&
            u.UserName == userName)), Times.Never);       
    }
    
    [Fact]
    public async Task ResetPassword_WhenPasswordAlreadyUpdatedByAnotherUser_ThrowsConflictException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string newPassword = "new password";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword
        };

        ResetPasswordRequestDto resetPasswordRequest = new ResetPasswordRequestDto
        {
            NewPassword = newPassword,
            ConfirmationPassword = newPassword
        };
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.ResetPasswordAsync(userId, newPassword, It.IsAny<CancellationToken>()))
            .Callback<int, string, CancellationToken>((id, password, _) =>
                {
                    user.PasswordHash = password;
                })
            .ReturnsAsync(0);
        
        Func<Task> result = () => _authService.ResetPassword(resetPasswordRequest, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);       
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ResetPasswordAsync(userId, newPassword, It.IsAny<CancellationToken>()), Times.Once);
        _mailFactoryService.Verify(x => x.CreatePasswordChangedMail(It.Is<User>(u =>
            u.Email == userEmail &&
            u.UserName == userName)), Times.Never);       
    }
    
    [Fact]
    public async Task VerifyPendingUser_WhenUserExists_ChangesUserStatusToActive()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string verificationToken = "test verification token";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
            Status = UserStatus.Pending
        };

        _verificationTokenService.Setup(x =>
                x.ValidateVerificationToken(verificationToken, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.ChangeUserStatusAsync(user, UserStatus.Active, It.IsAny<CancellationToken>()))
            .Callback<User, UserStatus, CancellationToken>((u, s, __) =>
            {
                u.Status = s;
            })
            .Returns(Task.CompletedTask);
        
        await _authService.VerifyPendingUser(verificationToken, userId, CancellationToken.None);
        
        Assert.Equal(UserStatus.Active, user.Status);
        
        _verificationTokenService.Verify(x => x.ValidateVerificationToken(verificationToken, userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ChangeUserStatusAsync(user, UserStatus.Active, It.IsAny<CancellationToken>()), Times.Once);      
    }
    
    [Fact]
    public async Task VerifyPendingUser_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string verificationToken = "test verification token";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
            Status = UserStatus.Pending
        };
        
        _verificationTokenService.Setup(x =>
                x.ValidateVerificationToken(verificationToken, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _authService.VerifyPendingUser(verificationToken, userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);       
        
        _verificationTokenService.Verify(x => x.ValidateVerificationToken(verificationToken, userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.ChangeUserStatusAsync(user, UserStatus.Active, It.IsAny<CancellationToken>()), Times.Never);      
    }
    
    [Fact]
    public async Task ResendVerificationMail_WhenUserExists_SendsVerificationMail()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string userPassword = "test password";
        string verificationToken = "test verification token";

        User user = new User
        {
            Id = userId,
            UserName = userName,
            Email = userEmail,
            PasswordHash = userPassword,
            Status = UserStatus.Pending
        };

        MailData mailData = new MailData
        {
            EmailToId = userEmail,
            EmailToName = userName,
            EmailSubject = "verification",
            EmailBody = "test"
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _verificationTokenService.Setup(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(verificationToken);
        _mailFactoryService.Setup(x => x.CreateVerificationMail(It.Is<User>(u =>
                u.Email == userEmail &&
                u.UserName == userName), verificationToken))
            .Returns(mailData);

        await _authService.ResendVerificationMail(userId, CancellationToken.None);

        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Once);
        _mailFactoryService.Verify(x => x.CreateVerificationMail(It.Is<User>(u =>
                u.Email == userEmail &&
                u.UserName == userName), verificationToken), Times.Once);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Once);
    }
    
    [Fact]
    public async Task ResendVerificationMail_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        string userName = "test user";
        string userEmail = "test email";
        string verificationToken = "test verification token";
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> result = () => _authService.ResendVerificationMail(userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);       
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _verificationTokenService.Verify(x => x.GenerateVerificationToken(userId, It.IsAny<CancellationToken>()), Times.Never);
        _mailFactoryService.Verify(x => x.CreateVerificationMail(It.Is<User>(u =>
            u.Email == userEmail &&
            u.UserName == userName), verificationToken), Times.Never);
        _backgroundJobClient.Verify(x => x.Create(
                It.Is<Job>(job =>
                    job.Type == typeof(IMailService) &&
                    job.Method.Name == nameof(IMailService.SendMail)),
                It.IsAny<EnqueuedState>()),
            Times.Never);
    }

    [Fact]
    public async Task VerifyPasswordChange_WhenUserExists_VerifiesVerificationToken()
    {
        int userId = 1;
        string verificationToken = "test verification token";
        string newPassword = "new password";

        ResetPasswordRequestDto resetPasswordRequest = new ResetPasswordRequestDto
        {
            NewPassword = newPassword,
            ConfirmationPassword = newPassword
        };
        
        _verificationTokenService.Setup(x => x.ValidateVerificationToken(verificationToken, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        await _authService.VerifyPasswordChange(verificationToken, resetPasswordRequest, CancellationToken.None);
        
        _verificationTokenService.Verify(x => x.ValidateVerificationToken(verificationToken, userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyPasswordChange_WhenPasswordDoesNotMatch_ThrowsUnauthorizedException()
    {
        int userId = 1;
        string verificationToken = "test verification token";
        string newPassword = "new password";
        string confirmationPassword = "wrong password";
        
        ResetPasswordRequestDto resetPasswordRequest = new ResetPasswordRequestDto
        {
            NewPassword = newPassword,
            ConfirmationPassword = confirmationPassword
        };
        
        Func<Task> result = () => _authService.VerifyPasswordChange(verificationToken, resetPasswordRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<UnauthorizedException>(result);       
        
        _verificationTokenService.Verify(x => x.ValidateVerificationToken(verificationToken, userId, It.IsAny<CancellationToken>()), Times.Never);
    }
}