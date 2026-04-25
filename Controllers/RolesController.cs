using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.DTOs.Responses;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleResponseDto>>> GetRoles()
        {
            List<Role> roles = await _roleService.GetRoles();
            List<RoleResponseDto> roleResponses = roles.Select(role => new RoleResponseDto
            {
                Name = role.Name
            }).ToList();
            return Ok(roleResponses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleResponseDto>> GetRoleById(int id)
        {
            Role role = await _roleService.GetRoleById(id);
            RoleResponseDto roleResponse = new RoleResponseDto
            {
                Name = role.Name
            };
            return Ok(roleResponse);
        }

        [HttpPost]
        public async Task<ActionResult<RoleResponseDto>> AddRole(RoleRequestDto roleRequest)
        {
            Role role = await _roleService.AddRole(roleRequest.Name);
            RoleResponseDto roleResponse = new RoleResponseDto
            {
                Name = role.Name
            };
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, roleResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleById(int id, RoleRequestDto roleRequest)
        {
            await _roleService.UpdateRoleById(id, roleRequest.Name);
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleById(int id)
        {
           await _roleService.DeleteRoleById(id);
           return NoContent();
        }

        [HttpPost("{roleId}/permissions/{permissionId}")]
        public async Task<ActionResult<Role>> AddPermissionToRole(int roleId, int permissionId)
        {
            Role role = await _roleService.AddPermissionToRole(roleId, permissionId);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }
        
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            await _roleService.RemovePermissionFromRole(roleId, permissionId);
            return NoContent();
        }
    }
}