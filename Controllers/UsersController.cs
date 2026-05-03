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
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<PageResult<UserResponseDto>>> GetUsers([FromQuery] UserQueryRequestDto userQueryRequest)
        {
            PageResult<User> usersPageResult = await _userService.GetUsers(
                userQueryRequest.PageIndex, 
                userQueryRequest.PageSize,
                userQueryRequest.Status
            );
            PageResult<UserResponseDto> userResponsesPageResult = UserMapper.ToUserResponsesPageResult(usersPageResult);
            return Ok(userResponsesPageResult);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id)
        {
            User user = await _userService.GetUserById(id);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return Ok(userResponse);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserResponseDto>> GetUserProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            User user = await _userService.GetUserById(userId);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return Ok(userResponse);
        }

        [HttpPost("{id}/ban")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> BanUser(int id)
        {
            await _userService.BanUser(id);
            return NoContent();
        }

        [HttpPost("{id}/unban")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            await _userService.UnbanUser(id);
            return NoContent();
        }

        [HttpPost("{userId}/roles/{roleId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserResponseDto>> AddRoleToUser(int userId, int roleId)
        {
            User user = await _userService.AddRoleToUser(userId, roleId);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userResponse);
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            await _userService.RemoveRoleFromUser(userId, roleId);
            return NoContent();
        }
    }
}