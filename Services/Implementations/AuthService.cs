using PersonalKnowledgeHub.BackgroundTasks;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Exceptions;
using PersonalKnowledgeHub.Repositories.Interfaces;
using PersonalKnowledgeHub.Services.Interfaces;
using PersonalKnowledgeHub.Mapper;
using Hangfire;

namespace PersonalKnowledgeHub.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWorkRepository _unitOfWorkRepository;
        private readonly IMailFactoryService _mailFactoryService;
        private readonly IVerificationTokenService _verificationTokenService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, 
            IUnitOfWorkRepository unitOfWorkRepository, IMailFactoryService mailFactoryService, 
            IVerificationTokenService verificationTokenService, IBackgroundJobClient backgroundJobClient)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _unitOfWorkRepository = unitOfWorkRepository;
            _mailFactoryService = mailFactoryService;
            _verificationTokenService = verificationTokenService;
            _backgroundJobClient = backgroundJobClient;
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

        public async Task<AuthResponseDto> RegisterUser(RegisterRequestDto registerRequest, CancellationToken cancellationToken)
        {
            string email = registerRequest.Email.Trim().ToLower();
            bool valid = IsEmailValid(email);
            if (!valid)
            {
                throw new ValidationException("Email is invalid");
            }
            bool exist = await _userRepository.IsEmailExistAsync(email, cancellationToken);
            if (exist)
            {
                throw new ConflictException("Email already existed");
            }
            User user = UserMapper.ToUser(registerRequest);
            User registeredUser = await _userRepository.AddUserAsync(user, cancellationToken);
            string refreshToken = await _tokenService.GenerateRefreshToken(registeredUser.Id, Guid.NewGuid(), cancellationToken);
            string accessToken = await _tokenService.GenerateAccessToken(registeredUser.Id, cancellationToken);
            string verificationToken = await _verificationTokenService.GenerateVerificationToken(registeredUser.Id, cancellationToken);
            MailData verificationMail = _mailFactoryService.CreateVerificationMail(user, verificationToken);
            _backgroundJobClient.Enqueue<IMailService>(mailService => mailService.SendMail(verificationMail));
            return AuthMapper.ToAuthResponseDto(refreshToken, accessToken);
        }

        public async Task<AuthResponseDto> AuthenticateUser(LoginRequestDto loginRequest, CancellationToken cancellationToken)
        {
            User user = UserMapper.ToUser(loginRequest);
            user.Email = user.Email.Trim().ToLower();
            User? loggedInUser = await _userRepository.GetUserByEmailAsync(user.Email, cancellationToken);
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
                await _userRepository.UpdateFailedLoginAttemptsAsync(loggedInUser.Id, 5, 2, cancellationToken);
                throw new UnauthorizedException("Password is incorrect");
            }
            await _userRepository.ResetFailedLoginAttemptsAsync(loggedInUser.Id, cancellationToken);
            string refreshToken = await _tokenService.GenerateRefreshToken(loggedInUser.Id, Guid.NewGuid(), cancellationToken);
            string accessToken = await _tokenService.GenerateAccessToken(loggedInUser.Id, cancellationToken);
            return AuthMapper.ToAuthResponseDto(refreshToken, accessToken);
        }

        public async Task<AuthResponseDto> RefreshUser(RefreshRequestDto refreshRequest, CancellationToken cancellationToken)
        {
            await using var transaction = await _unitOfWorkRepository.BeginTransactionAsync(cancellationToken);
            try
            {
                RefreshToken refreshToken = await _tokenService.ValidateRefreshToken(refreshRequest.RefreshToken, cancellationToken);
                string newRefreshTokenString = await _tokenService.GenerateRefreshToken(refreshToken.UserId, refreshToken.FamilyId, cancellationToken);
                RefreshToken newRefreshToken = await _tokenService.GetRefreshToken(newRefreshTokenString, cancellationToken);
                await _tokenService.RevokeRefreshToken(refreshToken.Token, newRefreshToken.Id, cancellationToken);
                string accessToken = await _tokenService.GenerateAccessToken(refreshToken.UserId, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return AuthMapper.ToAuthResponseDto(newRefreshTokenString, accessToken);
            }
            catch (NotFoundException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new NotFoundException(ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                await transaction.CommitAsync(cancellationToken);
                throw new UnauthorizedException(ex.Message);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task LogoutUser(LogoutRequestDto logoutRequest, int userId, CancellationToken cancellationToken)
        {
            RefreshToken refreshToken = await _tokenService.GetRefreshToken(logoutRequest.RefreshToken, cancellationToken);
            if (refreshToken.UserId != userId)
            {
                throw new ForbiddenException("Refresh token belongs to another user");
            }
            await _tokenService.RevokeRefreshToken(refreshToken.Token, null, cancellationToken);
        }

        public async Task ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequest, CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetUserByEmailAsync(forgotPasswordRequest.Email, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedException("Email is incorrect");
            }
            string passwordResetToken = await _verificationTokenService.GenerateVerificationToken(user.Id, cancellationToken);
            MailData passwordResetMail = _mailFactoryService.CreatePasswordResetMail(user, passwordResetToken);
            _backgroundJobClient.Enqueue<IMailService>(mailService => mailService.SendMail(passwordResetMail));
        }

        public async Task ResetPassword(ResetPasswordRequestDto resetPasswordRequest, int userId, CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmationPassword)
            {
                throw new ConflictException("Passwords do not match");
            }
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.NewPassword);
            int updateRows = await _userRepository.ResetPasswordAsync(userId, hashedPassword, cancellationToken);
            if (updateRows == 0)
            {
                throw new ConflictException("User has been updated by another user");
            }
            MailData passwordChangedMail = _mailFactoryService.CreatePasswordChangedMail(user);
            _backgroundJobClient.Enqueue<IMailService>(mailService => mailService.SendMail(passwordChangedMail));
        }

        public async Task VerifyPendingUser(string token, int userId, CancellationToken cancellationToken)
        {
            await _verificationTokenService.ValidateVerificationToken(token, userId, cancellationToken);
            User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            await _userRepository.ChangeUserStatusAsync(user, UserStatus.Active, cancellationToken);
        }

        public async Task ResendVerificationMail(int userId, CancellationToken cancellationToken)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }
            string verificationToken = await _verificationTokenService.GenerateVerificationToken(user.Id, cancellationToken);
            MailData verificationMail = _mailFactoryService.CreateVerificationMail(user, verificationToken);
            _backgroundJobClient.Enqueue<IMailService>(mailService => mailService.SendMail(verificationMail));
        }

        public async Task<int> VerifyPasswordChange(string token, ResetPasswordRequestDto resetPasswordRequest, CancellationToken cancellationToken)
        {
            if (resetPasswordRequest.NewPassword != resetPasswordRequest.ConfirmationPassword)
            {
                throw new ConflictException("Passwords do not match");
            }
            return await _verificationTokenService.ValidatePasswordResetToken(token, cancellationToken);
        }
    }
}