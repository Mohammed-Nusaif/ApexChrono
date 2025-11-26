using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EComApi.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        /// <summary>
        /// Returns the currently logged-in user's profile with role information.
        /// </summary>
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (UserId == null)
            {
                return Unauthorized();
            }
            //load User
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Fetch roles from Identity tables
            var roles = await _userManager.GetRolesAsync(user);

            // Determine if admin
            bool isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

            // Build profile object
            var profile = new
            {
                user.Id,
                user.UserName,
                user.Email,
                Roles = roles,
                IsAdmin = isAdmin
            };
            return Ok(profile);
        }
        /// <summary>
        /// Returns whether the current user is an Admin.
        /// </summary>
        /// 
        [HttpGet("IsAdmin")]
        public async Task<IActionResult> CheckIfAdmin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }
            var roles = await _userManager.GetRolesAsync(user);
            bool isAdmin = roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

            return Ok(new { IsAdmin = isAdmin });
        }
    }
}
