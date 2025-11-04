using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static EComApi.Entity.Models.Order;

namespace EComApi.Controllers.Admin
{
    [Route("api/Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;
        public DashboardController(ApplicationDbContext context, IRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue("username");
            var roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);

            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine($"Username: {userName}");
            Console.WriteLine($"Roles in token: {string.Join(", ", roles)}");

            // Check if user is admin via database
            var isAdminFromDb = await _roleService.IsUserAdmin(userId);
            Console.WriteLine($"Is Admin from DB: {isAdminFromDb}");

            if (!await IsCurrentUserAdmin())
            {
                Console.WriteLine("IsCurrentUserAdmin returned FALSE");
                return Forbid();
            }

            Console.WriteLine("IsCurrentUserAdmin returned TRUE - proceeding...");

            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var totalUsers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    o.User.UserName,
                    o.TotalAmount,
                    o.Status,
                    o.OrderDate
                })
                .ToListAsync();

            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts
            });

        }

        private async Task<bool> IsCurrentUserAdmin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _roleService.IsUserAdmin(userId);
        }
    }
}
