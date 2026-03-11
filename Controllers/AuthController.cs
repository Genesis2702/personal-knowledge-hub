using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Services.Interfaces;

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
        public async Task<IActionResult> Register(RegisterRequestDto registerRequest)
        {
            await _authService.RegisterUser(registerRequest);
            return Created("", "User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            bool authenticated = await _authService.AuthenticateUser(loginRequest);
            if (!authenticated)
            {
                return Unauthorized();
            }
            return Ok();
        }
    }
}
