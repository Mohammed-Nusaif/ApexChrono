using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static EComApi.Entity.Models.Order;

namespace EComApi.Controllers.Admin
{
    [Route("api/Admin/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;

        public OrdersController(ApplicationDbContext context, IRoleService roleService)
        {
            _context = context;
           _roleService = roleService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new AdminOrderDto
                {
                    OrderId = o.Id,
                    UserId = o.UserId,
                    UserName = o.User.UserName,
                    UserEmail = o.User.Email,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    ShippingAddress = o.ShippingAddress,
                    CustomerPhone = o.CustomerPhone,
                    RazorpayOrderId = o.RazorpayOrderId,
                    RazorpayPaymentId = o.RazorpayPaymentId,
                    OrderItems = o.OrderItems.Select(oi => new AdminOrderItemDto
                    {
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var orderDto = new AdminOrderDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                UserName = order.User.UserName,
                UserEmail = order.User.Email,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                CustomerPhone = order.CustomerPhone,
                RazorpayOrderId = order.RazorpayOrderId,
                RazorpayPaymentId = order.RazorpayPaymentId,
                OrderItems = order.OrderItems.Select(oi => new AdminOrderItemDto
                {
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };

            return Ok(orderDto);
        }
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse<OrderStatus>(statusDto.Status, out var newStatus))
            {
                return BadRequest("Invalid order status");
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully" });
        }
        [HttpGet("stats")]
        public async Task<IActionResult> GetOrderStats()
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
            var totalRevenue = await _context.Orders.Where(o => o.Status == OrderStatus.Confirmed).SumAsync(o => o.TotalAmount);

            return Ok(new
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders,
                TotalRevenue = totalRevenue
            });
        }

        private async Task<bool> IsCurrentUserAdmin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _roleService.IsUserAdmin(userId);
        }
    }
    public class AdminOrderDto
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public List<AdminOrderItemDto> OrderItems { get; set; } = new();
    }

    public class AdminOrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; }
    }
}
