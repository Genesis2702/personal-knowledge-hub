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
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PermissionResponseDto>>> GetPermissions()
        {
            List<Permission> permissions = await _permissionService.GetPermissions();
            List<PermissionResponseDto> permissionResponses = permissions.Select(permission => new PermissionResponseDto
            {
                Name = permission.Name
            }).ToList();
            return Ok(permissionResponses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionResponseDto>> GetPermissionById(int id)
        {
            Permission permission = await _permissionService.GetPermissionById(id);
            PermissionResponseDto permissionResponse = new PermissionResponseDto
            {
                Name = permission.Name
            };
            return Ok(permissionResponse);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionResponseDto>> AddPermission(PermissionRequestDto permissionRequest)
        {
            Permission permission = await _permissionService.AddPermission(permissionRequest.Name);
            PermissionResponseDto permissionResponse = new PermissionResponseDto
            {
                Name = permission.Name
            };
            return CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, permissionResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermissionById(int id, PermissionRequestDto permissionRequest)
        {
            await _permissionService.UpdatePermissionById(id, permissionRequest.Name);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermissionById(int id)
        {
            await _permissionService.DeletePermissionById(id);
            return NoContent();
        }
    }
}