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
    public async Task GetUsers_WhenUsersExist_ReturnsListOfUsers()
    {
        int pageIndex = 1;
        int pageSize = 2;
        UserStatus? status = UserStatus.Active;

        int userCount = 3;

        List<User> users = [
            new User
            {
                Id = 1,
                UserName = "test user1",
                Email = "test email1",
                PasswordHash = "test password1",
                Status = UserStatus.Active
            },
            new User
            {
                Id = 2,
                UserName = "test user2",
                Email = "test email2",
                PasswordHash = "test password2",
                Status = UserStatus.Active
            },
            new User
            {
                Id = 3,
                UserName = "test user3",
                Email = "test email3",
                PasswordHash = "test password3",
                Status = UserStatus.Active
            }
        ];

        _userRepository.Setup(x => x.GetUsersAsync(pageIndex, pageSize, status, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, userCount));

        var result = await _userService.GetUsers(pageIndex, pageSize, status, CancellationToken.None);

        Assert.NotEmpty(result.Items);
        Assert.Equal(result.PageCount, (int)Math.Ceiling((double)userCount / pageSize));
        Assert.Equal(result.PageIndex, pageIndex);
        Assert.Equal(result.PageSize, pageSize);
        
        _userRepository.Verify(x => x.GetUsersAsync(pageIndex, pageSize, status, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUsers_WhenNoUsersExist_ReturnsEmptyList()
    {
        int pageIndex = 1;
        int pageSize = 2;
        UserStatus? status = UserStatus.Active;

        int userCount = 0;
        
        List<User> users = new List<User>();
        
        _userRepository.Setup(x => x.GetUsersAsync(pageIndex, pageSize, status, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users, userCount));
        
        var result = await _userService.GetUsers(pageIndex, pageSize, status, CancellationToken.None);
        
        Assert.Empty(result.Items);
        Assert.Equal(result.PageCount, (int)Math.Ceiling((double)userCount / pageSize));
        Assert.Equal(result.PageIndex, pageIndex);
        Assert.Equal(result.PageSize, pageSize);
        
        _userRepository.Verify(x => x.GetUsersAsync(pageIndex, pageSize, status, It.IsAny<CancellationToken>()), Times.Once);
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
    
    [Fact]
    public async Task BanUser_WhenUserExists_BansUser()
    {
        int userId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            Status = UserStatus.Active,
            BannedAt = null
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.BanUserAsync(user, It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((bannedUser, _) =>
            {
                bannedUser.Status = UserStatus.Banned;
                bannedUser.BannedAt = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);
        _tokenRepository.Setup(x => x.RevokeRefreshTokensByUserAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        await _userService.BanUser(userId, CancellationToken.None);
        
        Assert.Equal(UserStatus.Banned, user.Status);
        Assert.NotNull(user.BannedAt);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.BanUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _tokenRepository.Verify(x => x.RevokeRefreshTokensByUserAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BanUser_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _userService.BanUser(userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.BanUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _tokenRepository.Verify(x => x.RevokeRefreshTokensByUserAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnbanUser_WhenUserExists_UnbansUser()
    {
        int userId = 1;
        
        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            Status = UserStatus.Banned,
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepository.Setup(x => x.UnbanUserAsync(user, It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((unbannedUser, _) =>
            {
                unbannedUser.Status = UserStatus.Active;
                unbannedUser.BannedAt = null;
            })
            .Returns(Task.CompletedTask);
        
        await _userService.UnbanUser(userId, CancellationToken.None);
        
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Null(user.BannedAt);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.UnbanUserAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnbanUser_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _userService.UnbanUser(userId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.UnbanUserAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task AddRoleToUser_WhenUserAndRoleExist_AddsRoleToUser()
    {
        int userId = 1;
        int roleId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            UserRoles = new List<UserRole>()
        };

        Role role = new Role
        {
            Id = roleId,
            Name = "test role"
        };

        UserRole userRole = new UserRole
        {
            User = user,
            Role = role,
            UserId = userId,
            RoleId = roleId
        };

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _userRepository.Setup(x => x.AddRoleToUserAsync(It.Is<UserRole>(ur => ur.UserId == userId && ur.RoleId == roleId), It.IsAny<CancellationToken>()))
            .Callback<UserRole, CancellationToken>((ur, _) =>
            {
                user.UserRoles.Add(ur);
            })
            .ReturnsAsync(user);
        
        var result = await _userService.AddRoleToUser(userId, roleId, CancellationToken.None);
        
        Assert.Contains(user.UserRoles, ur => ur.UserId == userId && ur.RoleId == roleId);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.AddRoleToUserAsync(It.Is<UserRole>(ur => ur.UserId == userId && ur.RoleId == roleId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRoleToUser_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _userService.AddRoleToUser(userId, 1, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(x => x.AddRoleToUserAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddRoleToUser_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        int roleId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            UserRoles = new List<UserRole>()
        };
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        
        Func<Task> result = () => _userService.AddRoleToUser(userId, roleId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.AddRoleToUserAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenUserAndRoleAndUserRoleExist_RemovesRoleFromUser()
    {
        int userId = 1;
        int roleId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            UserRoles = new List<UserRole>()
        };
        
        Role role = new Role
        {
            Id = roleId,
            Name = "test role"
        };

        UserRole userRole = new UserRole
        {
            User = user,
            Role = role,
            UserId = userId,
            RoleId = roleId
        };
        
        user.UserRoles.Add(userRole);

        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _userRepository.Setup(x => x.GetUserRoleAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);
        _userRepository.Setup(x => x.RemoveRoleFromUserAsync(userRole, It.IsAny<CancellationToken>()))
            .Callback<UserRole, CancellationToken>((ur, _) =>
            {
                user.UserRoles.Remove(ur);
            })
            .Returns(Task.CompletedTask);
        
        await _userService.RemoveRoleFromUser(userId, roleId, CancellationToken.None);
        
        Assert.DoesNotContain(user.UserRoles, ur => ur.UserId == userId && ur.RoleId == roleId);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.GetUserRoleAsync(userId, roleId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.RemoveRoleFromUserAsync(userRole, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        
        Func<Task> result = () => _userService.RemoveRoleFromUser(userId, 1, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(x => x.GetUserRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);       
        _userRepository.Verify(x => x.RemoveRoleFromUserAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);       
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        int roleId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            UserRoles = new List<UserRole>()
        };
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);
        
        Func<Task> result = () => _userService.RemoveRoleFromUser(userId, roleId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.GetUserRoleAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);       
        _userRepository.Verify(x => x.RemoveRoleFromUserAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);      
    }

    [Fact]
    public async Task RemoveRoleFromUser_WhenUserRoleDoesNotExist_ThrowsNotFoundException()
    {
        int userId = 1;
        int roleId = 1;

        User user = new User
        {
            Id = userId,
            UserName = "test user",
            Email = "test email",
            PasswordHash = "test password",
            UserRoles = new List<UserRole>()
        };

        Role role = new Role
        {
            Id = roleId,
            Name = "test role"
        };
        
        _userRepository.Setup(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _roleRepository.Setup(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _userRepository.Setup(x => x.GetUserRoleAsync(userId, roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRole?)null);
        
        Func<Task> result = () => _userService.RemoveRoleFromUser(userId, roleId, CancellationToken.None);
        
        await Assert.ThrowsAsync<NotFoundException>(result);
        
        _userRepository.Verify(x => x.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _roleRepository.Verify(x => x.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(x => x.GetUserRoleAsync(userId, roleId, It.IsAny<CancellationToken>()), Times.Once);       
        _userRepository.Verify(x => x.RemoveRoleFromUserAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never);      
    }
}