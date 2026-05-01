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
        private readonly IMailService _mailService;
        private readonly IMailFactoryService _mailFactoryService;
        private readonly IVerificationTokenService _verificationTokenService;

        public AuthService(IUserRepository userRepository, ITokenRepository tokenRepository, 
            ITokenService tokenService, IUnitOfWorkRepository unitOfWorkRepository, 
            IMailFactoryService mailFactoryService, IMailService mailService,
            IVerificationTokenService verificationTokenService)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _tokenService = tokenService;
            _unitOfWorkRepository = unitOfWorkRepository;
            _mailFactoryService = mailFactoryService;
            _mailService = mailService;
            _verificationTokenService = verificationTokenService;
        }

        public bool IsEmailValid(string email)
        {
            if (!email.Contains("@")) return false;
            List<char> specialChar = new List<char>() { '.', '_', '%', '+', '-' };
            string local = email.Substring(0, email.IndexOf('@'));
            for (int i = 0; i < local.Length; i++)
            {
                if (!(local[i] >= 'a' && local[i] <= 'z') && !(local[i] >= '0' && local[i] <= '9') && !(specialChar.Contains(local[i])))
                {
                    return false;
                }
            }
            string domain = email.Substring(email.IndexOf('@') + 1);
            if (domain != "gmail.com") return false;
            return true;
        }

        public async Task<AuthResponseDto> RegisterUser(RegisterRequestDto registerRequest)
        {
            string email = registerRequest.Email.Trim().ToLower();
            bool valid = IsEmailValid(email);
            if (!valid)
            {
                throw new ValidationException("Email is invalid");
            }
            bool exist = await _userRepository.IsEmailExistAsync(email);
            if (exist)
            {
                throw new ConflictException("Email already existed");
            }
            User user = UserMapper.ToUser(registerRequest);
            User registeredUser = await _userRepository.AddUserAsync(user);
            string refreshToken = await _tokenService.GenerateRefreshToken(registeredUser.Id, Guid.NewGuid());
            string accessToken = await _tokenService.GenerateAccessToken(registeredUser.Id);
            string verificationToken = await _verificationTokenService.GenerateVerificationToken(registeredUser.Id);
            MailData verificationMail = _mailFactoryService.CreateVerificationMail(registeredUser.Email, 
                registeredUser.UserName ?? registeredUser.Email, 
                verificationToken, registeredUser.UserName ?? registeredUser.Email);
            bool mailResult = await _mailService.SendMail(verificationMail);
            if (!mailResult)
            {
                throw new Exception("Mail sending failed");
            }
            return AuthMapper.ToAuthResponseDto(refreshToken, accessToken);
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
            string refreshToken = await _tokenService.GenerateRefreshToken(loggedInUser.Id, Guid.NewGuid());
            string accessToken = await _tokenService.GenerateAccessToken(loggedInUser.Id);
            await _tokenRepository.CleanUpRefreshTokenAsync();
            return AuthMapper.ToAuthResponseDto(refreshToken, accessToken);
        }

        public async Task<AuthResponseDto> RefreshUser(RefreshRequestDto refreshRequest)
        {
            await using var transaction = await _unitOfWorkRepository.BeginTransactionAsync();
            try
            {
                RefreshToken refreshToken = await _tokenService.ValidateRefreshToken(refreshRequest.RefreshToken);
                string newRefreshTokenString = await _tokenService.GenerateRefreshToken(refreshToken.UserId, refreshToken.FamilyId);
                RefreshToken newRefreshToken = await _tokenService.GetRefreshToken(newRefreshTokenString);
                await _tokenService.RevokeRefreshToken(refreshToken.Token, newRefreshToken.Id);
                string accessToken = await _tokenService.GenerateAccessToken(refreshToken.UserId);
                await transaction.CommitAsync();
                return AuthMapper.ToAuthResponseDto(newRefreshTokenString, accessToken);
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
            User? user = await _userRepository.GetUserByIdAsync(userId);
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
            MailData resetPasswordMail = _mailFactoryService.CreateResetPasswordMail(user.Email, user.UserName ?? user.Email, user.UserName ?? user.Email);
            bool mailResult = await _mailService.SendMail(resetPasswordMail);
            if (!mailResult)
            {
                throw new Exception("Mail sending failed");
            }
        }

        public async Task VerifyPendingUser(string token, int userId)
        {
            await _verificationTokenService.ValidateVerificationToken(token, userId);
            User? user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            await _userRepository.ChangeUserStatusAsync(user, UserStatus.Active);
        }
    }
}