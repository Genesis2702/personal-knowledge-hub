using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Mapper;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, 
        ITokenRepository tokenRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tokenRepository = tokenRepository;
        _logger = logger;
    }

    public async Task<PageResult<User>> GetUsers(int pageIndex, int pageSize, UserStatus? status, CancellationToken cancellationToken)
    {
        (List<User> users, int usersCount) = await _userRepository.GetUsersAsync(pageIndex, pageSize, status, cancellationToken);
        PageResult<User> usersPageResult = UserMapper.ToUsersPageResult(users, usersCount, pageIndex, pageSize);
        return usersPageResult;
    }

    public async Task<User> GetUserById(int id, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        return user;
    }

    public async Task UpdateUserName(int id, UserUpdateRequestDto userUpdateRequest, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByIdAsync(id, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        int updatedRows = await _userRepository.UpdateUserNameAsync(id, user.Version, userUpdateRequest.UserName, cancellationToken);
        if (updatedRows == 0)
        {
            throw new ConflictException("User was updated by another user");
        }
    }
    
    public async Task BanUser(int userId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        await _userRepository.BanUserAsync(user, cancellationToken);
        await _tokenRepository.RevokeRefreshTokensByUserAsync(user.Id, cancellationToken);
    }

    public async Task UnbanUser(int userId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        await _userRepository.UnbanUserAsync(user, cancellationToken);
    }

    public async Task<User> AddRoleToUser(int userId, int roleId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        Role? role = await _roleRepository.GetRoleByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        UserRole userRole = new UserRole
        {
            User = user,
            Role = role,
            UserId = userId,
            RoleId = roleId
        };
        _logger.LogInformation("Role {name} added to user {userId} successfully", role.Name, user.Id);
        return await _userRepository.AddRoleToUserAsync(userRole, cancellationToken);
    }

    public async Task RemoveRoleFromUser(int userId, int roleId, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        Role? role = await _roleRepository.GetRoleByIdAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        UserRole? userRole = await _userRepository.GetUserRoleAsync(userId, roleId, cancellationToken);
        if (userRole == null)
        {
            throw new NotFoundException("User role not found");
        }
        await _userRepository.RemoveRoleFromUserAsync(userRole, cancellationToken);
        _logger.LogInformation("Role {name} removed from user {userId} successfully", role.Name, user.Id);
    }
}