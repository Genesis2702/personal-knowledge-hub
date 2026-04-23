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
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshRequestDto refreshRequest)
        {
            AuthResponseDto authResponse = await _authService.RefreshUser(refreshRequest);
            return Ok(authResponse);
        }
        
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(LogoutRequestDto logoutRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _authService.LogoutUser(logoutRequest, userId);
            return Ok();
        }

        [HttpDelete("ban-user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Ban(int id)
        {
            await _authService.BanUser(id);
            return Ok();
        }

        [HttpPost("unban-user/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Unban(int id)
        {
            await _authService.UnbanUser(id);
            return Ok();
        }
        
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto resetPasswordRequest)
        {
            await _authService.ResetPassword(resetPasswordRequest);
            return Ok("Password reset successfully");
        }
    }
}
