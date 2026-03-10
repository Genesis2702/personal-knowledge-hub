using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _authRepository;

        public AuthService(IUserRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public bool IsEmailValid(string email)
        {
            if (!email.Contains("@")) return false;
            List<char> specialChar = new List<char>() { '.', '_', '%', '+', '-' };
            string local = email.Substring(0, email.IndexOf("@"));
            for (int i = 0; i < local.Length; i++)
            {
                if (!(local[i] >= 'a' && local[i] <= 'z') && !(specialChar.Contains(local[i])))
                {
                    return false;
                }
            }
            string domain = email.Substring(email.IndexOf("@") + 1);
            if (domain != "gmail.com") return false;
            return true;
        }

        public async Task RegisterUser(User user)
        {
            user.Email = user.Email.Trim().ToLower();
            bool valid = IsEmailValid(user.Email);
            if (!valid)
            {
                throw new Exception("Email is invalid");
            }
            bool exist = await _authRepository.IsEmailExistAsync(user.Email);
            if (exist == true)
            {
                throw new Exception("Email already existed");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            await _authRepository.AddUserAsync(user);
        }

        public async Task<bool> AuthenticateUser(User user)
        {
            var account = await _authRepository.GetUserAsync(user.Email);
            if (account == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, account.PasswordHash))
            {
                return false;
            }
            return true;
        }
    }
}