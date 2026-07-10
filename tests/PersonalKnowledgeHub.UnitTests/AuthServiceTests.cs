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
}