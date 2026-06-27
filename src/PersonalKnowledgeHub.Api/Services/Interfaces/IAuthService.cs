using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IAuthService
    {
        public bool IsEmailValid(string email);
        public Task<AuthResponseDto> RegisterUser(RegisterRequestDto registerRequest, CancellationToken cancellationToken);
        public Task<AuthResponseDto> AuthenticateUser(LoginRequestDto loginRequest, CancellationToken cancellationToken);
        public Task<AuthResponseDto> RefreshUser(RefreshRequestDto refreshRequest, CancellationToken cancellationToken);
        public Task LogoutUser(LogoutRequestDto logoutRequest, int userId, CancellationToken cancellationToken);
        public Task ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequest, CancellationToken cancellationToken);
        public Task ResetPassword(ResetPasswordRequestDto resetPasswordRequest, int userId, CancellationToken cancellationToken);
        public Task VerifyPendingUser(string token, int userId, CancellationToken cancellationToken);
        public Task ResendVerificationMail(int userId, CancellationToken cancellationToken);
        public Task<int> VerifyPasswordChange(string token, ResetPasswordRequestDto resetPasswordRequest, CancellationToken cancellationToken);
    }
}
