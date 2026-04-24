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

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
}