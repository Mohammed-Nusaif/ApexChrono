using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EComApi.Entity.Models;

namespace EComApi.Controllers.Setup
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleManager : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManager(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        // 1️⃣ CREATE NEW ROLE
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest model)
        {
            if (string.IsNullOrWhiteSpace(model.RoleName))
                return BadRequest("Role name cannot be empty.");

            if (await _roleManager.RoleExistsAsync(model.RoleName))
                return BadRequest("Role already exists.");

            var result = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));

            if (result.Succeeded)
                return Ok($"✅ Role '{model.RoleName}' created successfully.");

            return BadRequest(result.Errors);
        }

        // 2️⃣ LIST ALL ROLES
        [HttpGet("list")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToList();
            return Ok(roles);
        }

        // 3️⃣ DELETE A ROLE
        [HttpDelete("delete/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return NotFound("Role not found.");

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
                return Ok($"✅ Role '{roleName}' deleted successfully.");

            return BadRequest(result.Errors);
        }
    }
}
