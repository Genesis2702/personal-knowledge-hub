using PersonalKnowledgeHub.DTOs.Requests;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IAuthService
    {
        public bool IsEmailValid(string email);
        public Task RegisterUser(RegisterRequestDto registerRequest);
        public Task<bool> AuthenticateUser(LoginRequestDto loginRequest);
    }
}
