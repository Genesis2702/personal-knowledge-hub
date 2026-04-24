using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.Common;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Mapper;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "ActiveAccount")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<PageResult<User>>> GetUsers([FromQuery] UserQueryRequestDto userQueryRequest)
        {
            PageResult<User> usersPageResult = await _userService.GetUsers(
                userQueryRequest.PageIndex, 
                userQueryRequest.PageSize,
                userQueryRequest.Status
            );
            return Ok(usersPageResult);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            User user = await _userService.GetUserById(id);
            return Ok(user);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> GetUserProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            User user = await _userService.GetUserById(userId);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return Ok(userResponse);
        }

        [HttpPost("{id}/ban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser(int id)
        {
            await _userService.BanUser(id);
            return NoContent();
        }

        [HttpPost("{id}/unban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            await _userService.UnbanUser(id);
            return NoContent();
        }
    }
}