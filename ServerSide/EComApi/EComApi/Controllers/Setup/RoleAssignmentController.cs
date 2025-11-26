using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EComApi.Controllers.Setup
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleAssignmentController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleAssignmentController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // POST /api/setup/assign-role
        [HttpPost]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return NotFound("User not found.");

            if (!await _roleManager.RoleExistsAsync(model.Role))
                return BadRequest("Role does not exist.");

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            if (result.Succeeded)
                return Ok($"✅ User '{model.Email}' assigned to role '{model.Role}'.");
            else
                return BadRequest(result.Errors);
        }

        // GET /api/setup/assign-role/{email}
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound("User not found.");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new { email = user.Email, roles });
        }
    }
}
