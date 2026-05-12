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
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PermissionResponseDto>>> GetPermissions(CancellationToken cancellationToken)
        {
            List<Permission> permissions = await _permissionService.GetPermissions(cancellationToken);
            List<PermissionResponseDto> permissionResponses = permissions.Select(permission => new PermissionResponseDto
            {
                Name = permission.Name
            }).ToList();
            return Ok(permissionResponses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionResponseDto>> GetPermissionById(int id, CancellationToken cancellationToken)
        {
            Permission permission = await _permissionService.GetPermissionById(id, cancellationToken);
            PermissionResponseDto permissionResponse = new PermissionResponseDto
            {
                Name = permission.Name
            };
            return Ok(permissionResponse);
        }

        [HttpPost]
        public async Task<ActionResult<PermissionResponseDto>> AddPermission(PermissionRequestDto permissionRequest, CancellationToken cancellationToken)
        {
            Permission permission = await _permissionService.AddPermission(permissionRequest.Name, cancellationToken);
            PermissionResponseDto permissionResponse = new PermissionResponseDto
            {
                Name = permission.Name
            };
            return CreatedAtAction(nameof(GetPermissionById), new { id = permission.Id }, permissionResponse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermissionById(int id, PermissionRequestDto permissionRequest, CancellationToken cancellationToken)
        {
            await _permissionService.UpdatePermissionById(id, permissionRequest.Name, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermissionById(int id, CancellationToken cancellationToken)
        {
            await _permissionService.DeletePermissionById(id, cancellationToken);
            return NoContent();
        }
    }
}