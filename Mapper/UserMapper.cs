using PersonalKnowledgeHub.DTOs.Requests;
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
}