using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _authRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository authRepository, ITokenService tokenService)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
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

        public async Task<AuthResponseDto> RegisterUser(RegisterRequestDto registerRequest)
        {
            User user = new User
            {
                UserName = registerRequest.UserName ?? registerRequest.Email,
                Email = registerRequest.Email,
                PasswordHash = registerRequest.Password
            };
            user.Email = user.Email.Trim().ToLower();
            bool valid = IsEmailValid(user.Email);
            if (!valid)
            {
                throw new InvalidCredentialException("Email is invalid");
            }
            bool exist = await _authRepository.IsEmailExistAsync(user.Email);
            if (exist == true)
            {
                throw new InvalidCredentialException("Email already existed");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            User registeredUser = await _authRepository.AddUserAsync(user);
            RefreshToken refreshToken = await _tokenService.GenerateRefreshToken(registeredUser.Id);
            AuthResponseDto authResponse = new AuthResponseDto
            {
                RefreshToken = refreshToken.Token,
                AccessToken = await _tokenService.GenerateAccessToken(registeredUser.Id)
            };
            return authResponse;
        }

        public async Task<AuthResponseDto> AuthenticateUser(LoginRequestDto loginRequest)
        {
            User user = new User
            {
                Email = loginRequest.Email,
                PasswordHash = loginRequest.Password
            };
            user.Email = user.Email.Trim().ToLower();
            User? loggedInUser = await _authRepository.GetUserByEmailAsync(user.Email);
            if (loggedInUser == null)
            {
                throw new UnauthorizedException("Email is incorrect");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.PasswordHash, loggedInUser.PasswordHash))
            {
                throw new UnauthorizedException("Password is incorrect");
            }
            RefreshToken refreshToken = await _tokenService.GenerateRefreshToken(loggedInUser.Id);
            AuthResponseDto authResponse = new AuthResponseDto
            {
                RefreshToken = refreshToken.Token,
                AccessToken = await _tokenService.GenerateAccessToken(loggedInUser.Id)
            };
            return authResponse;
        }
    }
}