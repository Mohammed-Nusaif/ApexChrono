using EComApi.Entity.DTO.User;
using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EComApi.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // ---------------------------------------------------------
        // GET ALL USERS
        // ---------------------------------------------------------
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userManager.Users.ToListAsync();
            var list = new List<UserDto>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                list.Add(new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address,
                    City = u.City,
                    Country = u.Country,
                    PostalCode = u.PostalCode,
                    ProfileImageUrl = u.ProfileImageUrl,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt,
                    IsActive = u.IsActive,
                    IsVerified = u.IsVerified,
                    Roles = roles.ToList()
                });
            }

            return Ok(list);
        }
        // ---------------------------------------------------------
        // GET USER BY ID
        // ---------------------------------------------------------
        [HttpGet("GetUsersbyId/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);

            return Ok(new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email!,
                PhoneNumber = u.PhoneNumber,
                Address = u.Address,
                City = u.City,
                Country = u.Country,
                PostalCode = u.PostalCode,
                ProfileImageUrl = u.ProfileImageUrl,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                IsActive = u.IsActive,
                IsVerified = u.IsVerified,
                Roles = roles.ToList()
            });
        }
        // ---------------------------------------------------------
        // UPDATE USER
        // ---------------------------------------------------------
        [HttpPut("UpdateUserbyId/{id}")]
        public async Task<IActionResult> Update(string id, UpdateUserDto model)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            u.FullName = model.FullName ?? u.FullName;
            u.PhoneNumber = model.PhoneNumber ?? u.PhoneNumber;
            u.Address = model.Address ?? u.Address;
            u.City = model.City ?? u.City;
            u.Country = model.Country ?? u.Country;
            u.PostalCode = model.PostalCode ?? u.PostalCode;
            if (model.IsActive.HasValue) u.IsActive = model.IsActive.Value;
            if (model.IsVerified.HasValue) u.IsVerified = model.IsVerified.Value;

            var res = await _userManager.UpdateAsync(u);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok("User updated");
        }
        // ---------------------------------------------------------
        // BLOCK / UNBLOCK USER
        // ---------------------------------------------------------
        [HttpPatch("{id}/BlockUser")]
        public async Task<IActionResult> Block(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            u.IsActive = !u.IsActive;
            await _userManager.UpdateAsync(u);

            return Ok(u.IsActive ? "User Unblocked" : "User Blocked");
        }
        // ---------------------------------------------------------
        // ASSIGN ROLES (MULTIPLE ROLES)
        // ---------------------------------------------------------
        [HttpPatch("{id}/Assignroles")]
        public async Task<IActionResult> AssignRoles(string id, AssignRolesDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(u);

            // ensure roles exist
            foreach (var r in dto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(r))
                    await _roleManager.CreateAsync(new IdentityRole(r));
            }

            await _userManager.RemoveFromRolesAsync(u, currentRoles);
            await _userManager.AddToRolesAsync(u, dto.Roles);

            return Ok("Roles updated");
        }
        // ---------------------------------------------------------
        // RESET PASSWORD (Admin Sets New Password)
        // ---------------------------------------------------------
        [HttpPatch("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, ResetPasswordDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(u);
            var result = await _userManager.ResetPasswordAsync(u, token, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok("Password reset");
        }
        // ---------------------------------------------------------
        // DELETE USER
        // ---------------------------------------------------------
        [HttpDelete("{id}/DeleteUser")]
        public async Task<IActionResult> Delete(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var res = await _userManager.DeleteAsync(u);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok("Deleted");
        }

    }
}
