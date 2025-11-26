using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static EComApi.Entity.Models.Order;

namespace EComApi.Controllers.Admin
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Dashboard Summary API
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // --- Overall Counts ---
            var totalUsers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalVariants = await _context.ProductVariants.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();

            // --- Revenue ---
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            // --- User Growth (last 30 days) ---
            var usersLastMonth = await _context.Users
                .Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30))
                .CountAsync();

            // --- Monthly Revenue Trend (SAFE FIX APPLIED) ---
            var monthlyRevenueRaw = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Delivered)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalOrders = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .Take(6)
                .ToListAsync();

            var monthlyRevenue = monthlyRevenueRaw.Select(x => new
            {
                Month = $"{x.Month:D2}-{x.Year}",
                x.TotalOrders,
                x.TotalRevenue
            });

            // --- Recent Orders ---
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    CustomerName = o.User.UserName,
                    o.TotalAmount,
                    Status = o.Status.ToString(),
                    o.OrderDate
                })
                .ToListAsync();

            // --- Low Stock Check ---
            var lowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Brand,
                    p.Stock,
                    IsVariant = false
                })
                .ToListAsync();

            var lowStockVariants = await _context.ProductVariants
                .Where(v => v.Stock < 10)
                .Select(v => new
                {
                    v.Id,
                    Name = v.Color,
                    Brand = "Variant",
                    v.Stock,
                    IsVariant = true
                })
                .ToListAsync();

            var combinedLowStock = lowStockProducts.Concat(lowStockVariants).Take(10).ToList();

            // --- Top Selling Products (OPTION 1 FIX APPLIED) ---
            var topSelling = await _context.OrderItems
                .GroupBy(oi => new { oi.ProductId, oi.ProductName })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    TotalSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice) // FIXED — WORKING IN EF
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                Summary = new
                {
                    TotalUsers = totalUsers,
                    UsersLastMonth = usersLastMonth,
                    TotalProducts = totalProducts,
                    TotalVariants = totalVariants,
                    TotalOrders = totalOrders,
                    TotalRevenue = totalRevenue
                },
                Trends = new
                {
                    MonthlyRevenue = monthlyRevenue
                },
                RecentOrders = recentOrders,
                LowStockAlerts = combinedLowStock,
                TopSellingProducts = topSelling
            });
        }
    }
}
