using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("controller")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("/auth/register")]
        public async Task<IActionResult> Register(RegisterRequestDto registerRequest)
        {
            var user = new User
            {
                UserName = registerRequest.UserName != null ? registerRequest.UserName : registerRequest.Email,
                Email = registerRequest.Email,
                PasswordHash = registerRequest.Password
            };
            await _authService.RegisterUser(user);
            return Created("", "User registered successfully");
        }

        [HttpPost("/auth/login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            var user = new User
            {
                Email = loginRequest.Email,
                PasswordHash = loginRequest.Password
            };
            bool authenticated = await _authService.AuthenticateUser(user);
            if (!authenticated)
            {
                return Unauthorized();
            }
            return Ok();
        }
    }
}
