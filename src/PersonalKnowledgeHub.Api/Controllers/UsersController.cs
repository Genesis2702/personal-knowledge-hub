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
        public async Task<ActionResult<PageResult<UserResponseDto>>> GetUsers([FromQuery] UserQueryRequestDto userQueryRequest, CancellationToken cancellationToken)
        {
            PageResult<User> usersPageResult = await _userService.GetUsers(
                userQueryRequest.PageIndex, 
                userQueryRequest.PageSize,
                userQueryRequest.Status,
                cancellationToken
            );
            PageResult<UserResponseDto> userResponsesPageResult = UserMapper.ToUserResponsesPageResult(usersPageResult);
            return Ok(userResponsesPageResult);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id, CancellationToken cancellationToken)
        {
            User user = await _userService.GetUserById(id, cancellationToken);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return Ok(userResponse);
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserResponseDto>> GetUserProfile(CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            User user = await _userService.GetUserById(userId, cancellationToken);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return Ok(userResponse);
        }

        [HttpPatch("profile")]
        public async Task<IActionResult> UpdateUserProfile(UserUpdateRequestDto userUpdateRequest, CancellationToken cancellationToken)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _userService.UpdateUserName(userId, userUpdateRequest, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/ban")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> BanUser(int id, CancellationToken cancellationToken)
        {
            await _userService.BanUser(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id}/unban")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UnbanUser(int id, CancellationToken cancellationToken)
        {
            await _userService.UnbanUser(id, cancellationToken);
            return NoContent();
        }

        [HttpPost("{userId}/roles/{roleId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<UserResponseDto>> AddRoleToUser(int userId, int roleId, CancellationToken cancellationToken)
        {
            User user = await _userService.AddRoleToUser(userId, roleId, cancellationToken);
            UserResponseDto userResponse = UserMapper.ToUserResponseDto(user);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, userResponse);
        }

        [HttpDelete("{userId}/roles/{roleId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId, CancellationToken cancellationToken)
        {
            await _userService.RemoveRoleFromUser(userId, roleId, cancellationToken);
            return NoContent();
        }
    }
}