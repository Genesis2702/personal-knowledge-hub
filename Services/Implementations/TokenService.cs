using Microsoft.IdentityModel.Tokens;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public TokenService(ITokenRepository tokenRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            _tokenRepository = tokenRepository;
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<RefreshToken> GenerateRefreshToken(int userId)
        {
            User user = (await _userRepository.GetUserByIdAsync(userId))!;
            RefreshToken refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Revoked = false,
                RevokedAt = null,
                UserId = user.Id,
                User = user
            };
            return refreshToken;
        }

        public async Task<string> GenerateAccessToken(int userId)
        {
            User user = (await _userRepository.GetUserByIdAsync(userId))!;
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public async Task<bool> RevokeRefreshToken(string token)
        {
            return await _tokenRepository.RevokeRefreshTokenAsync(token);
        }

        public async Task<bool> ValidateRefreshToken(string token)
        {
            var refreshToken = await _tokenRepository.GetRefreshTokenAsync(token);
            if (refreshToken == null) { return false; }
            if (refreshToken.Revoked == true) { return false; }
            if (refreshToken.ExpiresAt < DateTime.UtcNow) { return false; }
            return true;
        }
    }
}
