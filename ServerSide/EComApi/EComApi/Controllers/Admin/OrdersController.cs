using EComApi.Entity.DTO.Order;
using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using static EComApi.Entity.Models.Order;

namespace EComApi.Controllers.Admin
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------
        // 🔹 Helper method to avoid null propagation inside expressions
        // ---------------------------------------------------------
        private static string GetImageSafe(ProductVariant? variant, Products? product)
        {
            if (variant?.ImageUrls != null && variant.ImageUrls.Any())
                return variant.ImageUrls.First();

            if (product?.ImageUrls != null && product.ImageUrls.Any())
                return product.ImageUrls.First();

            return "";
        }

        // ---------------------------------------------------------
        // ✅ 1. Get ALL Orders (Null-Safe)
        // ---------------------------------------------------------
        [HttpGet("getAllOrders")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.ProductVariant)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync(); // Important: materialize result before mapping

            var result = orders.Select(o => new AdminOrderDto
            {
                OrderId = o.Id,
                UserId = o.UserId,
                UserName = o.User?.UserName ?? "Unknown",
                UserEmail = o.User?.Email ?? "Unknown",
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
                    VariantColor = oi.VariantColor,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    ImageUrl = GetImageSafe(oi.ProductVariant, oi.Product)
                }).ToList()

            }).ToList();

            return Ok(result);
        }

        // ---------------------------------------------------------
        // ✅ 2. Get Order By ID (Null-Safe)
        // ---------------------------------------------------------
        [HttpGet("GerOrderById/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { success = false, message = "Order not found" });

            var orderDto = new AdminOrderDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                UserName = order.User?.UserName ?? "Unknown",
                UserEmail = order.User?.Email ?? "Unknown",
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
                    VariantColor = oi.VariantColor,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    ImageUrl = GetImageSafe(oi.ProductVariant, oi.Product)
                }).ToList()
            };

            return Ok(orderDto);
        }

        // ---------------------------------------------------------
        // ✅ 3. Update order status
        // ---------------------------------------------------------
        [HttpPut("{id}/UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound(new { success = false, message = "Order not found" });

            if (!Enum.TryParse<OrderStatus>(statusDto.Status, out var newStatus))
                return BadRequest(new { success = false, message = "Invalid order status" });

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Order status updated successfully" });
        }

        // ---------------------------------------------------------
        // ✅ 4. Cancel Order (restock items)
        // ---------------------------------------------------------
        [HttpPost("{id}/CancelOrder")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound(new { success = false, message = "Order not found" });

            if (order.Status == OrderStatus.Delivered)
                return BadRequest(new { success = false, message = "Cannot cancel a delivered order" });

            order.Status = OrderStatus.Cancelled;

            // Restock items
            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariantId.HasValue)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
                    if (variant != null)
                        variant.Stock += item.Quantity;
                }
                else
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                        product.Stock += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Order cancelled and stock restored" });
        }

        // ---------------------------------------------------------
        // ✅ 5. Stats
        // ---------------------------------------------------------
        [HttpGet("GetOrderStats")]
        public async Task<IActionResult> GetOrderStats()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var confirmedOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed);
            var deliveredOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered);

            var totalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed || o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            var monthlyStats = await _context.Orders
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Month:D2}-{g.Key.Year}",
                    Orders = g.Count(),
                    Revenue = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(g => g.Month)
                .ToListAsync();

            return Ok(new
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                ConfirmedOrders = confirmedOrders,
                DeliveredOrders = deliveredOrders,
                TotalRevenue = totalRevenue,
                MonthlyStats = monthlyStats
            });
        }
    }
}
