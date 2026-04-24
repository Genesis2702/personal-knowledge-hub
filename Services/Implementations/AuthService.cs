using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Mapper;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        public AuthService(IUserRepository userRepository, ITokenRepository tokenRepository, 
            ITokenService tokenService, IUnitOfWorkRepository unitOfWorkRepository)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _tokenService = tokenService;
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        public bool IsEmailValid(string email)
        {
            if (!email.Contains("@")) return false;
            List<char> specialChar = new List<char>() { '.', '_', '%', '+', '-' };
            string local = email.Substring(0, email.IndexOf("@"));
            for (int i = 0; i < local.Length; i++)
            {
                if (!(local[i] >= 'a' && local[i] <= 'z') && !(local[i] >= '0' && local[i] <= '9') && !(specialChar.Contains(local[i])))
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
            User user = UserMapper.ToUser(registerRequest);
            user.Email = user.Email.Trim().ToLower();
            bool valid = IsEmailValid(user.Email);
            if (!valid)
            {
                throw new ValidationException("Email is invalid");
            }
            bool exist = await _userRepository.IsEmailExistAsync(user.Email);
            if (exist)
            {
                throw new ConflictException("Email already existed");
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.Status = UserStatus.Pending;
            user.BannedAt = null;
            User registeredUser = await _userRepository.AddUserAsync(user);
            RefreshToken refreshToken = await _tokenService.GenerateRefreshToken(registeredUser.Id, Guid.NewGuid());
            string accessToken = await _tokenService.GenerateAccessToken(registeredUser.Id);
            return AuthMapper.ToAuthResponseDto(refreshToken.Token, accessToken);
        }

        public async Task<AuthResponseDto> AuthenticateUser(LoginRequestDto loginRequest)
        {
            User user = UserMapper.ToUser(loginRequest);
            user.Email = user.Email.Trim().ToLower();
            User? loggedInUser = await _userRepository.GetUserByEmailAsync(user.Email);
            if (loggedInUser == null)
            {
                throw new UnauthorizedException("Email is incorrect");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.PasswordHash, loggedInUser.PasswordHash))
            {
                throw new UnauthorizedException("Password is incorrect");
            }
            RefreshToken refreshToken = await _tokenService.GenerateRefreshToken(loggedInUser.Id, Guid.NewGuid());
            string accessToken = await _tokenService.GenerateAccessToken(loggedInUser.Id);
            await _tokenRepository.CleanUpRefreshTokenAsync();
            return AuthMapper.ToAuthResponseDto(refreshToken.Token, accessToken);
        }

        public async Task<AuthResponseDto> RefreshUser(RefreshRequestDto refreshRequest)
        {
            await using var transaction = await _unitOfWorkRepository.BeginTransactionAsync();
            try
            {
                RefreshToken refreshToken = await _tokenService.ValidateRefreshToken(refreshRequest.RefreshToken);
                RefreshToken newRefreshToken =
                    await _tokenService.GenerateRefreshToken(refreshToken.UserId, refreshToken.FamilyId);
                await _tokenService.RevokeRefreshToken(refreshToken.Token, newRefreshToken.Id);
                string accessToken = await _tokenService.GenerateAccessToken(refreshToken.UserId);
                await transaction.CommitAsync();
                return AuthMapper.ToAuthResponseDto(newRefreshToken.Token, accessToken);
            }
            catch (NotFoundException ex)
            {
                await transaction.RollbackAsync();
                throw new NotFoundException(ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                await transaction.CommitAsync();
                throw new UnauthorizedException(ex.Message);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task LogoutUser(LogoutRequestDto logoutRequest, int userId)
        {
            RefreshToken refreshToken = await _tokenService.GetRefreshToken(logoutRequest.RefreshToken);
            if (refreshToken.UserId != userId)
            {
                throw new ForbiddenException("Refresh token belongs to another user");
            }
            await _tokenService.RevokeRefreshToken(logoutRequest.RefreshToken, null);
        }

        public async Task ForgotPassword(int userId, string newPassword)
        {
            User? user = await _userRepository.GetUserByEmailAsync(newPassword);
            if (user == null)
            {
                throw new UnauthorizedException("Email is incorrect");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.ResetPasswordAsync(user, hashedPassword);
        }

        public async Task ResetPassword(ResetPasswordRequestDto resetPasswordRequest)
        {
            User? user = await _userRepository.GetUserByEmailAsync(resetPasswordRequest.Email);
            if (user == null)
            {
                throw new UnauthorizedException("Email is incorrect");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.NewPassword);
            await _userRepository.ResetPasswordAsync(user, hashedPassword);
        }
    }
}