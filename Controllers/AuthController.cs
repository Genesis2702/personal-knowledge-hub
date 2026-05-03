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

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto registerRequest)
        {
            AuthResponseDto authResponse = await _authService.RegisterUser(registerRequest);
            return Created("", authResponse);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto loginRequest)
        {
            AuthResponseDto authResponse = await _authService.AuthenticateUser(loginRequest);
            return Ok(authResponse);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshRequestDto refreshRequest)
        {
            AuthResponseDto authResponse = await _authService.RefreshUser(refreshRequest);
            return Ok(authResponse);
        }
        
        [HttpPost("logout")]
        [Authorize(Policy = "ActiveAccount,PendingAccount")]
        public async Task<IActionResult> Logout(LogoutRequestDto logoutRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.LogoutUser(logoutRequest, userId);
            return Ok();
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto forgotPasswordRequest)
        {
            await _authService.ForgotPassword(forgotPasswordRequest);
            return Ok("Password reset mail sent");
        }

        [HttpPost("change-password")]
        [Authorize(Policy = "ActiveAccount")]
        public async Task<IActionResult> ChangePassword(ResetPasswordRequestDto resetPasswordRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.ResetPassword(resetPasswordRequest, userId);
            return Ok("Password changed successfully");
        }
        
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromQuery] string token, ResetPasswordRequestDto resetPasswordRequest)
        {
            int userId = await _authService.VerifyPasswordChange(token, resetPasswordRequest);
            await _authService.ResetPassword(resetPasswordRequest, userId);
            return Ok("Password reset successfully");
        }

        [HttpPost("mail-verification")]
        [Authorize(Policy = "PendingAccount")]
        public async Task<IActionResult> VerifyMail([FromQuery] string token)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.VerifyPendingUser(token, userId);
            return Ok("Email verified successfully");
        }

        [HttpPost("resend-verification")]
        [Authorize(Policy = "PendingAccount")]
        public async Task<IActionResult> ResendMail()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.ResendVerificationMail(userId);
            return Ok("Verification mail resent");
        }
    }
}
