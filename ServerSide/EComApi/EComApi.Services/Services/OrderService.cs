using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO.Order;
using EComApi.Entity.Models;
using Microsoft.EntityFrameworkCore;
using static EComApi.Entity.Models.Order;

namespace EComApi.Services.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        // ✅ CREATE ORDER (variant-aware)
        public async Task<Result<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto createOrderDto)
        {
            var result = new Result<OrderDto>();
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Get user cart
                var cart = await _cartService.GetUserCartAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    result.Errors.Add(new Error { ErrorCode = 301, ErrorMessage = "Cart is empty" });
                    return result;
                }

                // 2. Create Order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    CustomerPhone = createOrderDto.CustomerPhone,
                    CustomerEmail = createOrderDto.CustomerEmail
                };

                decimal totalAmount = 0;

                // 3. Validate & process each cart item
                foreach (var cartItem in cart.Items)
                {
                    // Check if variant exists
                    var variant = await _context.ProductVariants
                        .Include(v => v.Product)
                        .FirstOrDefaultAsync(v => v.Id == cartItem.VariantId);

                    if (variant == null)
                    {
                        result.Errors.Add(new Error { ErrorCode = 302, ErrorMessage = $"Product variant not found for {cartItem.ProductName}" });
                        return result;
                    }

                    // Check stock
                    if (variant.Stock < cartItem.Quantity)
                    {
                        result.Errors.Add(new Error { ErrorCode = 303, ErrorMessage = $"Insufficient stock for {variant.Product.Name} ({variant.Color}). Available: {variant.Stock}" });
                        return result;
                    }

                    // Deduct stock
                    variant.Stock -= cartItem.Quantity;

                    // Create order item
                    var orderItem = new OrderItem
                    {
                        Order = order,
                        ProductId = variant.ProductId,
                        ProductVariantId = variant.Id,
                        ProductName = variant.Product.Name,
                        VariantColor = variant.Color,
                        Quantity = cartItem.Quantity,
                        UnitPrice = variant.Price,
                        //TotalPrice = variant.Price * cartItem.Quantity
                    };

                    order.OrderItems.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }

                // 4. Update total and save order
                order.TotalAmount = totalAmount;

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 5. Clear cart after successful order
                await _cartService.ClearCartAsync(userId);

                await transaction.CommitAsync();

                result.Response = MapToOrderDto(order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                result.Errors.Add(new Error { ErrorCode = 304, ErrorMessage = $"Failed to create order: {ex.Message}" });
            }

            return result;
        }

        // ✅ GET ORDER BY ID
        public async Task<Result<OrderDto>> GetOrderByIdAsync(string userId, int orderId)
        {
            var result = new Result<OrderDto>();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(v => v.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 307, ErrorMessage = "Order not found" });
                    return result;
                }

                result.Response = MapToOrderDto(order);
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 308, ErrorMessage = $"Error retrieving order: {ex.Message}" });
            }

            return result;
        }

        // ✅ GET ALL USER ORDERS
        public async Task<Result<List<OrderDto>>> GetUserOrdersAsync(string userId)
        {
            var result = new Result<List<OrderDto>>();
            try
            {
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                    .ThenInclude(v => v.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                result.Response = orders.Select(MapToOrderDto).ToList();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 306, ErrorMessage = $"Error retrieving orders: {ex.Message}" });
            }
            return result;
        }

        // ✅ UPDATE ORDER STATUS
        public async Task<Result<bool>> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var result = new Result<bool>();
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 309, ErrorMessage = "Order not found" });
                    return result;
                }

                order.Status = status;
                await _context.SaveChangesAsync();
                result.Response = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 310, ErrorMessage = $"Error updating order status: {ex.Message}" });
            }
            return result;
        }

        // ✅ MAP ORDER TO DTO (includes variant info)
        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddress = order.ShippingAddress,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemId = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    VariantId = oi.ProductVariantId,
                    VariantColor = oi.VariantColor,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }

        // Cancel the order 
        public async Task<Result<bool>> CancelOrderAsync(int orderId, string userId, bool isAdmin = false)
        {
            var result = new Result<bool>();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 310, ErrorMessage = "Order not found" });
                    return result;
                }

                // If user is not admin → ensure they own the order
                if (!isAdmin && order.UserId != userId)
                {
                    result.Errors.Add(new Error { ErrorCode = 311, ErrorMessage = "Unauthorized to cancel this order" });
                    return result;
                }

                // Order already cancelled
                if (order.Status == Order.OrderStatus.Cancelled)
                {
                    result.Errors.Add(new Error { ErrorCode = 312, ErrorMessage = "Order is already cancelled" });
                    return result;
                }

                // Delivered orders cannot be cancelled
                if (order.Status == Order.OrderStatus.Delivered)
                {
                    result.Errors.Add(new Error { ErrorCode = 313, ErrorMessage = "Delivered orders cannot be cancelled" });
                    return result;
                }

                // Reverse stock
                foreach (var item in order.OrderItems)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.ProductVariantId);
                    if (variant != null)
                    {
                        variant.Stock += item.Quantity;
                    }
                }

                // Update status
                order.Status = Order.OrderStatus.Cancelled;

                await _context.SaveChangesAsync();
                result.Response = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 314, ErrorMessage = $"Error canceling order: {ex.Message}" });
            }

            return result;
        }

    }
}
