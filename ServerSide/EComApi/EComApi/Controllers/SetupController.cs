using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EComApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetupController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SetupController(IRoleService roleService, UserManager<ApplicationUser> userManager)
        {
            _roleService = roleService;
            _userManager = userManager;
        }

        [HttpPost("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] string UserName)
        {
            var user = await _userManager.FindByNameAsync(UserName);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            // Check if user is already admin
            if (await _roleService.IsUserAdmin(user.Id))
            {
                return Ok(new { message = $"{UserName} is already an admin" });
            }

            var result = await _roleService.AssignAdminRole(user.Id);
            if (result)
            {
                return Ok(new { message = $"{UserName} is now an admin" });
            }

            return BadRequest("Failed to assign admin role");
        }
        [HttpGet("check-admin/{username}")]
        public async Task<IActionResult> CheckAdminStatus(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var isAdmin = await _roleService.IsUserAdmin(user.Id);
            var userRoles = await _roleService.GetUserRoles(user.Id);

            return Ok(new
            {
                Username = user.UserName,
                UserId = user.Id,
                IsAdmin = isAdmin,
                Roles = userRoles
            });
        }

        [HttpPost("initialize-roles")]
        public async Task<IActionResult> InitializeRoles()
        {
            await _roleService.InitializeRoles();
            return Ok(new { message = "Roles initialized successfully" });
        }

    }
}
