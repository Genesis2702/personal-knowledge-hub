using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Implementations;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly Mock<IRoleRepository> _roleRepository;
    private readonly Mock<ITokenRepository> _tokenRepository;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _roleRepository = new Mock<IRoleRepository>();
        _tokenRepository = new Mock<ITokenRepository>();
        _userService = new UserService(_userRepository.Object, _roleRepository.Object, _tokenRepository.Object, NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_ReturnsUser()
    {
        int userId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password"
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        var result = await _userService.GetUserById(userId, CancellationToken.None);
        
        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.UserName, result.UserName);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.PasswordHash, result.PasswordHash);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetUserById_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        Func<Task> result = () => _userService.GetUserById(userId, CancellationToken.None);

        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateUserName_WhenUserExists_UpdatesUserName()
    {
        int userId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            Version = 0
        };

        UserUpdateRequestDto userUpdateRequest = new UserUpdateRequestDto
        {
            UserName = "updated user"
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x =>
                x.UpdateUserNameAsync(userId, user.Version, userUpdateRequest.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        await _userService.UpdateUserName(userId, userUpdateRequest, CancellationToken.None);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.UpdateUserNameAsync(userId, user.Version, userUpdateRequest.UserName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserName_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;

        UserUpdateRequestDto userUpdateRequest = new UserUpdateRequestDto
        {
            UserName = "updated user"
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _userService.UpdateUserName(userId, userUpdateRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.UpdateUserNameAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task UpdateUserName_WhenUserNameAlreadyUpdatedByAnotherUser_ThrowsConflictException()
    {
        int userId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            Version = 0
        };

        UserUpdateRequestDto userUpdateRequest = new UserUpdateRequestDto
        {
            UserName = "updated user"
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x =>
                x.UpdateUserNameAsync(userId, user.Version, userUpdateRequest.UserName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        
        Func<Task> result = () => _userService.UpdateUserName(userId, userUpdateRequest, CancellationToken.None);
        
        await Assert.ThrowsAsync<ConflictException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.UpdateUserNameAsync(userId, user.Version, userUpdateRequest.UserName, It.IsAny<CancellationToken>()), Times.Once);
    }
}