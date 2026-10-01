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
    [Authorize(Roles = "ADMIN")]
    [Authorize(Policy = "ActiveAccount")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [EndpointSummary("Get all roles")]
        [HttpGet]
        public async Task<ActionResult<List<RoleResponseDto>>> GetRoles(CancellationToken cancellationToken)
        {
            List<Role> roles = await _roleService.GetRoles(cancellationToken);
            List<RoleResponseDto> roleResponses = roles.Select(role => new RoleResponseDto
            {
                Name = role.Name
            }).ToList();
            return Ok(roleResponses);
        }

        [EndpointSummary("Get a role by ID")]
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleResponseDto>> GetRoleById(int id, CancellationToken cancellationToken)
        {
            Role role = await _roleService.GetRoleById(id, cancellationToken);
            RoleResponseDto roleResponse = new RoleResponseDto
            {
                Name = role.Name
            };
            return Ok(roleResponse);
        }

        [EndpointSummary("Create a new role")]
        [HttpPost]
        public async Task<ActionResult<RoleResponseDto>> AddRole(RoleRequestDto roleRequest, CancellationToken cancellationToken)
        {
            Role role = await _roleService.AddRole(roleRequest.Name, cancellationToken);
            RoleResponseDto roleResponse = new RoleResponseDto
            {
                Name = role.Name
            };
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, roleResponse);
        }

        [EndpointSummary("Update a role by ID")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleById(int id, RoleRequestDto roleRequest, CancellationToken cancellationToken)
        {
            await _roleService.UpdateRoleById(id, roleRequest.Name, cancellationToken);
            return NoContent();
        }
        
        [EndpointSummary("Delete a role by ID")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleById(int id, CancellationToken cancellationToken)
        {
           await _roleService.DeleteRoleById(id, cancellationToken);
           return NoContent();
        }

        [EndpointSummary("Add a permission to a role")]
        [HttpPost("{roleId}/permissions/{permissionId}")]
        public async Task<ActionResult<RoleResponseDto>> AddPermissionToRole(int roleId, int permissionId, CancellationToken cancellationToken)
        {
            Role role = await _roleService.AddPermissionToRole(roleId, permissionId, cancellationToken);
            RoleResponseDto roleResponse = new RoleResponseDto
            {
                Name = role.Name
            };
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, roleResponse);
        }
        
        [EndpointSummary("Remove a permission from a role")]
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId, CancellationToken cancellationToken)
        {
            await _roleService.RemovePermissionFromRole(roleId, permissionId, cancellationToken);
            return NoContent();
        }
    }
}