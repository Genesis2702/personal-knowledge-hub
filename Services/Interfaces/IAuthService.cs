using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IAuthService
    {
        public bool IsEmailValid(string email);
        public Task<AuthResponseDto> RegisterUser(RegisterRequestDto registerRequest);
        public Task<AuthResponseDto> AuthenticateUser(LoginRequestDto loginRequest);
        public Task<AuthResponseDto> RefreshUser(RefreshRequestDto refreshRequest);
        public Task LogoutUser(LogoutRequestDto logoutRequest, int userId);
        public Task ForgotPassword(int userId, string newPassword);
        public Task ResetPassword(ResetPasswordRequestDto resetPasswordRequest);
        public Task VerifyPendingUser(string token, int userId);
    }
}
