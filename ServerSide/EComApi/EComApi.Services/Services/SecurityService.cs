using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EComApi.Services.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public SecurityService(UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _config = config;
        }
        public async Task<Result<ResponseDto>> Login(LoginDto login)
        {
            Result<ResponseDto> response = new();

            var user = await _userManager.FindByNameAsync(login.UserName);
            if (user == null)
            {
                response.Errors.Add(new Error { ErrorCode = 101, ErrorMessage = "User not found" });
                return response;
            }

            bool resultResponse = await _userManager.CheckPasswordAsync(user, login.Password);

            if (resultResponse)
            {

                string token = await GenerateJwtTokenAsync(user);

                ResponseDto userResponse = new()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    Token = token 
                };

                response.Response = userResponse;
            }
            else
            {
                response.Errors.Add(new Error { ErrorCode = 102, ErrorMessage = "Authentication Failed" });
            }

            return response;
        }

        public async Task<Result<ResponseDto>> Register(RequestDto request)
        {
            Result<ResponseDto> response = new();

            ApplicationUser user = new()
            {
                UserName = request.UserName,
                FullName = request.FullName,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
            };

            IdentityResult resultResponse = await _userManager.CreateAsync(user, request.Password);

            if (resultResponse.Succeeded)
            {
                ResponseDto userResponse = new()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    Token = null 
                };

                response.Response = userResponse;
            }
            else
            {
                foreach (var error in resultResponse.Errors)
                {
                    response.Errors.Add(new Error { ErrorCode = 102, ErrorMessage = error.Description });
                }
            }

            return response;
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("Jwt");
            // Get user roles
            var userRoles = await _userManager.GetRolesAsync(user);

            // Create claims including roles
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.UserName)
            };
            // Add role claims
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
