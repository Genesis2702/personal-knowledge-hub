using PersonalKnowledgeHub.Common;
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

    public UserService(IUserRepository userRepository, IRoleRepository roleRepository, ITokenRepository tokenRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tokenRepository = tokenRepository;
    }

    public async Task<PageResult<User>> GetUsers(int pageIndex, int pageSize, UserStatus? status)
    {
        (List<User> users, int usersCount) = await _userRepository.GetUsersAsync(pageIndex, pageSize, status);
        PageResult<User> usersPageResult = UserMapper.ToUsersPageResult(users, usersCount, pageIndex, pageSize);
        return usersPageResult;
    }

    public async Task<User> GetUserById(int id)
    {
        User? user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        return user;
    }
    
    public async Task BanUser(int userId)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        await _userRepository.BanUserAsync(user);
        await _tokenRepository.RevokeRefreshTokensByUserAsync(user.Id);
    }

    public async Task UnbanUser(int userId)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        await _userRepository.UnbanUserAsync(user);
    }

    public async Task<User> AddRoleToUser(int userId, int roleId)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        Role? role = await _roleRepository.GetRoleByIdAsync(roleId);
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
        return await _userRepository.AddRoleToUserAsync(userRole);
    }

    public async Task RemoveRoleFromUser(int userId, int roleId)
    {
        User? user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }
        Role? role = await _roleRepository.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            throw new NotFoundException("Role not found");
        }
        UserRole? userRole = await _userRepository.GetUserRoleAsync(userId, roleId);
        if (userRole == null)
        {
            throw new NotFoundException("User role not found");
        }
        await _userRepository.RemoveRoleFromUserAsync(userRole);
    }
}