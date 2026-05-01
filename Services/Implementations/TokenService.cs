using Microsoft.IdentityModel.Tokens;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

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

        public async Task<string> GenerateRefreshToken(int userId, Guid familyId)
        {
            string rawToken = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
            byte[] hashedToken = SHA256.HashData(binaryToken);
            RefreshToken refreshToken = new RefreshToken
            {
                Token = Convert.ToHexString(hashedToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                Revoked = false,
                RevokedAt = null,
                ReplacedByTokenId = null,
                FamilyId = familyId,
                UserId = userId
            };
            await _tokenRepository.AddRefreshTokenAsync(refreshToken);
            return rawToken;
        }

        public async Task<string> GenerateAccessToken(int userId)
        {
            User user = (await _userRepository.GetUserByIdAsync(userId))!;
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("status", user.Status.ToString())
            };
            claims.AddRange(
                user.UserRoles.Select(userRole => new Claim(ClaimTypes.Role, userRole.Role.Name)));
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public async Task RevokeRefreshToken(string token, int? replacedId)
        {
            await _tokenRepository.RevokeRefreshTokenAsync(token, replacedId);
        }

        public async Task<RefreshToken> ValidateRefreshToken(string rawToken)
        {
            byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
            byte[] hashedToken = SHA256.HashData(binaryToken);
            string token = Convert.ToHexString(hashedToken);
            RefreshToken? refreshToken = await _tokenRepository.GetRefreshTokenForUpdateAsync(token);
            if (refreshToken == null)
            {
                throw new NotFoundException("Refresh token not found");
            }
            if (refreshToken.Revoked)
            {
                await _tokenRepository.RevokeRefreshTokensByFamilyAsync(refreshToken.FamilyId, null);
                throw new UnauthorizedException("Refresh token reused detected");
            }
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                await _tokenRepository.RevokeRefreshTokenAsync(token, null);
                throw new UnauthorizedException("Refresh token is expired");
            }
            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshToken(string rawToken)
        {
            byte[] binaryToken = Encoding.UTF8.GetBytes(rawToken);
            byte[] hashedToken = SHA256.HashData(binaryToken);
            string token = Convert.ToHexString(hashedToken);
            RefreshToken? refreshToken = await _tokenRepository.GetRefreshTokenAsync(token);
            if (refreshToken == null) { throw new NotFoundException("Refresh token not found"); }
            return refreshToken;
        }
    }
}
