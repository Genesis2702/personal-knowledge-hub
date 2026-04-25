using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.Entities;
using PersonalKnowledgeHub.Services.Interfaces;

namespace PersonalKnowledgeHub.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Role>>> GetRoles()
        {
            List<Role> roles = await _roleService.GetRoles();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRoleById(int id)
        {
            Role role = await _roleService.GetRoleById(id);
            return Ok(role);
        }

        [HttpPost]
        public async Task<ActionResult<Role>> AddRole(RoleRequestDto roleRequest)
        {
            Role role = await _roleService.AddRole(roleRequest.Name);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleById(int id, RoleRequestDto roleRequest)
        {
            await _roleService.UpdateRoleById(id, roleRequest.Name);
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoleById(int id)
        {
           await _roleService.DeleteRoleById(id);
           return NoContent();
        }
    }
}