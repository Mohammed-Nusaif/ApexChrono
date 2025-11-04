using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
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
        public async Task<Result<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto createOrderDto)
        {
            var result = new Result<OrderDto>();
            try
            {
                // 1. Get user's cart
                var cart = await _cartService.GetUserCartAsync(userId);
                if (cart == null || !cart.Items.Any())
                {
                    result.Errors.Add(new Error { ErrorCode = 301, ErrorMessage = "Cart is empty" });
                    return result;
                }
                // 2. Validate stock and prices
                foreach (var cartItem in cart.Items)
                {
                    var product = await _context.Products.FindAsync(cartItem.ProductId);
                    if (product == null)
                    {
                        result.Errors.Add(new Error { ErrorCode = 302, ErrorMessage = $"Product {cartItem.ProductName} not found" });
                        return result;
                    }

                    if (product.Stock < cartItem.Quantity)
                    {
                        result.Errors.Add(new Error { ErrorCode = 303, ErrorMessage = $"Insufficient stock for {cartItem.ProductName}. Available: {product.Stock}" });
                        return result;
                    }

                    // Update stock (we'll complete this in transaction)
                    product.Stock -= cartItem.Quantity;
                }
                // 3. Create order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = cart.TotalAmount,
                    Status = OrderStatus.Pending,
                    ShippingAddress = createOrderDto.ShippingAddress,
                    CustomerPhone = createOrderDto.CustomerPhone,
                    CustomerEmail = createOrderDto.CustomerEmail
                };
                // 4. Create order items
                foreach (var cartItem in cart.Items)
                {
                    var product = await _context.Products.FindAsync(cartItem.ProductId);

                    var orderItem = new OrderItem
                    {
                        Order = order,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = product.Price, // Capture price at time of order
                        ProductName = product.Name // Snapshot of product name
                    };

                    order.OrderItems.Add(orderItem);
                }
                // 5. Save everything in transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Clear the cart after successful order creation
                    await _cartService.ClearCartAsync(userId);

                    await transaction.CommitAsync();

                    // 6. Return order DTO
                    var orderDto =  MapToOrderDto(order);
                    result.Response = orderDto;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Errors.Add(new Error { ErrorCode = 304, ErrorMessage = $"Failed to create order: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 305, ErrorMessage = $"Error creating order: {ex.Message}" });
            }
            return result;
        }

        public async Task<Result<OrderDto>> GetOrderByIdAsync(string userId, int orderId)
        {
            var result = new Result<OrderDto>();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 307, ErrorMessage = "Order not found" });
                    return result;
                }

                var orderDto = MapToOrderDto(order);
                result.Response = orderDto;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 308, ErrorMessage = $"Error retrieving order: {ex.Message}" });
            }

            return result;
        }

        public async Task<Result<List<OrderDto>>> GetUserOrdersAsync(string userId)
        {
            var result = new Result<List<OrderDto>>();

            try
            {
                var orders = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var orderDtos = orders.Select(MapToOrderDto).ToList();
                result.Response = orderDtos;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 306, ErrorMessage = $"Error retrieving orders: {ex.Message}" });
            }

            return result;
        }


        public async Task<Result<bool>> UpdateOrderStatusAsync(int orderId, Order.OrderStatus status)
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
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };
        }
    }
}
