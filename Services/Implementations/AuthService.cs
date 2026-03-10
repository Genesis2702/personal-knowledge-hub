using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
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
                if (!(local[i] >= 'a' && local[i] <= 'z') && !(local[i] >= 'A' && local[i] <= 'Z') && !(specialChar.Contains(local[i])))
                {
                    return false;
                }
            }
            string domain = email.Substring(email.IndexOf("@") + 1);
            string domainName = domain.Substring(0, domain.IndexOf("."));
            string topLevelDomain = domain.Substring(domain.IndexOf(".") + 1);
            if (domainName != "gmail" && topLevelDomain != "com") return false;
            return true;
        }

        public async Task RegisterUser(User user)
        {
            bool valid = IsEmailValid(user.Email);
            if (!valid)
            {
                throw new Exception("Email is invalid");
            }
            bool exist = await _authRepository.IsEmailExist(user.Email);
            if (exist == true)
            {
                throw new Exception("Email already existed");
            }
            string hashPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Password = hashPassword;
            user.CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow);
            await _authRepository.AddUserAsync(user);
        }
    }
}