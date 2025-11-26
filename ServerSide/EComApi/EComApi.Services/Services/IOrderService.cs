

using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO.Order;
using static EComApi.Entity.Models.Order;

namespace EComApi.Services.Services
{
    public interface IOrderService
    {
        Task<Result<OrderDto>> CreateOrderAsync(string userId, CreateOrderDto createOrderDto);
        Task<Result<List<OrderDto>>> GetUserOrdersAsync(string userId);
        Task<Result<OrderDto>> GetOrderByIdAsync(string userId, int orderId);
        Task<Result<bool>> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<Result<bool>> CancelOrderAsync(int orderId, string userId, bool isAdmin = false);


    }
}
