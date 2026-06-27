using PersonalKnowledgeHub.DTOs.Responses;

namespace PersonalKnowledgeHub.Mapper;

public class AuthMapper
{
    public static AuthResponseDto ToAuthResponseDto(string refreshToken, string accessToken)
    {
        return new AuthResponseDto
        {
            RefreshToken = refreshToken,
            AccessToken = accessToken
        };
    }
}