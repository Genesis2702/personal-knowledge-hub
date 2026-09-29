using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Services.Interfaces;
using System.Security.Claims;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [EndpointSummary("Register a new user account")]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto registerRequest, CancellationToken cancellationToken)
        {
            AuthResponseDto authResponse = await _authService.RegisterUser(registerRequest, cancellationToken);
            return Created("", authResponse);
        }

        [EndpointSummary("Authenticate a user")]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginRequest, CancellationToken cancellationToken)
        {
            AuthResponseDto authResponse = await _authService.AuthenticateUser(loginRequest, cancellationToken);
            return Ok(authResponse);
        }

        [EndpointSummary("Issue new tokens using refresh token")]
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshRequestDto refreshRequest, CancellationToken cancellationToken)
        {
            AuthResponseDto authResponse = await _authService.RefreshUser(refreshRequest, cancellationToken);
            return Ok(authResponse);
        }
        
        [EndpointSummary("Logout a user")]
        [HttpPost("logout")]
        [Authorize(Policy = "PendingOrActiveAccount")]
        public async Task<IActionResult> Logout(LogoutRequestDto logoutRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.LogoutUser(logoutRequest, userId, cancellationToken);
            return Ok();
        }

        [EndpointSummary("Send a password reset email")]
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequest, CancellationToken cancellationToken)
        {
            await _authService.ForgotPassword(forgotPasswordRequest, cancellationToken);
            return Ok("Password reset mail sent");
        }

        [EndpointSummary("Change current user's password")]
        [HttpPost("change-password")]
        [Authorize(Policy = "ActiveAccount")]
        public async Task<IActionResult> ChangePassword(ResetPasswordRequestDto resetPasswordRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.ResetPassword(resetPasswordRequest, userId, cancellationToken);
            return Ok("Password changed successfully");
        }
        
        [EndpointSummary("Reset password using reset password token")]
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, ResetPasswordRequestDto resetPasswordRequest, CancellationToken cancellationToken)
        {
            int userId = await _authService.VerifyPasswordChange(token, resetPasswordRequest, cancellationToken);
            await _authService.ResetPassword(resetPasswordRequest, userId, cancellationToken);
            return Ok("Password reset successfully");
        }

        [EndpointSummary("Verify current user's email address")]
        [HttpPost("mail-verification")]
        [Authorize(Policy = "PendingAccount")]
        public async Task<IActionResult> VerifyMail([FromQuery] string token, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.VerifyPendingUser(token, userId, cancellationToken);
            return Ok("Email verified successfully");
        }

        [EndpointSummary("Resend email verification message")]
        [HttpPost("resend-verification")]
        [Authorize(Policy = "PendingAccount")]
        public async Task<IActionResult> ResendMail(CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.ResendVerificationMail(userId, cancellationToken);
            return Ok("Verification mail resent");
        }
    }
}
