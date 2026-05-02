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
            UserName = registerRequest.UserName,
            Email = registerRequest.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
            CreatedAt = DateTime.UtcNow,
            Status = UserStatus.Pending,
            BannedAt = null,
            FailedLoginAttempts = 0,
            LockedUntil = null
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
            Status = user.Status,
            BannedAt = user.BannedAt,
            Resources = user.Resources.Select(resource => resource.Title).ToList(),
            Tags = user.Tags.Select(tag => tag.Name).ToList(),
            Roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToList(),
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
    
    public static PageResult<UserResponseDto> ToUserResponsesPageResult(PageResult<User> usersPageResult)
    {
        return new PageResult<UserResponseDto>
        {
            Items = usersPageResult.Items.Select(user => new UserResponseDto
            {
                UserName = user.UserName ?? user.Email,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Status = user.Status,
                BannedAt = user.BannedAt,
                Resources = user.Resources.Select(resource => resource.Title).ToList(),
                Tags = user.Tags.Select(tag => tag.Name).ToList(),
                Roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToList(),
            }).ToList(),
            PageIndex = usersPageResult.PageIndex,
            PageSize = usersPageResult.PageSize,
            PageCount = usersPageResult.PageCount
        };
    }
}