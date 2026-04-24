using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Mapper;

public class UserMapper
{
    public static User ToUser(RegisterRequestDto registerRequest)
    {
        return new User
        {
            UserName = registerRequest.UserName ?? registerRequest.Email,
            Email = registerRequest.Email,
            PasswordHash = registerRequest.Password
        };
    }
    
    public static User ToUser(LoginRequestDto loginRequest)
    {
        return new User
        {
            Email = loginRequest.Email,
            PasswordHash = loginRequest.Password
        };
    }

    public static UserResponseDto ToUserResponseDto(User user)
    {
        return new UserResponseDto
        {
            UserName = user.UserName ?? user.Email,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            IsBanned = user.IsBanned,
            BannedAt = user.BannedAt,
            Resources = user.Resources.Select(resource => resource.Title).ToList()
        };
    }
    
    public static PageResult<User> ToUsersPageResult(List<User> users, int usersCount, int pageIndex, int pageSize)
    {
        return new PageResult<User>
        {
            Items = users,
            PageIndex = pageIndex,
            PageSize = pageSize,
            PageCount = (int)Math.Ceiling((decimal)usersCount / pageSize)
        };
    }
}