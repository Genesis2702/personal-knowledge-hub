using PersonalKnowledgeHub.Entities;

namespace PersonalKnowledgeHub.Services.Interfaces
{
    public interface IAuthService
    {
        public bool IsEmailValid(string email);
        public Task RegisterUser(User user);
        public Task<bool> AuthenticateUser(User user);
    }
}
