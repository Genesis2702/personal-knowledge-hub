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
                throw new UnauthorizedException("User not found");
            }
            if (loggedInUser.LockedUntil != null && loggedInUser.LockedUntil > DateTime.UtcNow)
            {
                throw new UnauthorizedException("User is locked");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.PasswordHash, loggedInUser.PasswordHash))
            {
                await _userRepository.UpdateFailedLoginAttemptsAsync(loggedInUser.Id, 5, 2);
                throw new UnauthorizedException("Password is incorrect");
            }
            await _userRepository.ResetFailedLoginAttemptsAsync(loggedInUser.Id);
            string refreshToken = await _tokenService.GenerateRefreshToken(loggedInUser.Id, Guid.NewGuid());
            string accessToken = await _tokenService.GenerateAccessToken(loggedInUser.Id);
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
            await _tokenService.RevokeRefreshToken(refreshToken.Token, null);
        }

        public async Task ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequest)
        {
            User? user = await _userRepository.GetUserByEmailAsync(forgotPasswordRequest.Email);
            if (user == null)
            {
                throw new UnauthorizedException("Email is incorrect");
            }
            string passwordResetToken = await _verificationTokenService.GenerateVerificationToken(user.Id);
            MailData resetPasswordMail = _mailFactoryService.CreatePasswordResetMail(user.Email, 
                user.UserName ?? user.Email, passwordResetToken, user.UserName ?? user.Email);
            bool mailResult = await _mailService.SendMail(resetPasswordMail);
            if (!mailResult)
            {
                throw new Exception("Mail sending failed");
            }
        }

        public async Task ResetPassword(ResetPasswordRequestDto resetPasswordRequest, int userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmationPassword)
            {
                throw new ConflictException("Passwords do not match");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.NewPassword);
            await _userRepository.ResetPasswordAsync(user, hashedPassword);
            MailData passwordChangedMail = _mailFactoryService.CreatePasswordChangedMail(user.Email, user.UserName ?? user.Email, user.UserName ?? user.Email);
            bool mailResult = await _mailService.SendMail(passwordChangedMail);
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

        public async Task<int> VerifyPasswordChange(string token, ResetPasswordRequestDto resetPasswordRequest)
        {
            if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmationPassword)
            {
                throw new ConflictException("Passwords do not match");
            }
            return await _verificationTokenService.ValidatePasswordResetToken(token);
        }
    }
}