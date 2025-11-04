using EComApi.Entity.DTO;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static EComApi.Entity.Models.Order;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EComApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public OrderController(IOrderService orderService, IPaymentService paymentService)
        {
            _orderService = orderService;
            _paymentService = paymentService;
        }
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   User.FindFirstValue("sub");
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var result = await _orderService.CreateOrderAsync(userId, createOrderDto);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var result = await _orderService.GetUserOrdersAsync(userId);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }
        [HttpGet("GetOrderById{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            var userId = GetUserId();
            var result = await _orderService.GetOrderByIdAsync(userId, orderId);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }
        [HttpPut("{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
        {
            // Parse the string to OrderStatus enum
            if (!Enum.TryParse<OrderStatus>(status, out var orderStatus))
            {
                return BadRequest("Invalid order status");
            }

            var result = await _orderService.UpdateOrderStatusAsync(orderId, orderStatus);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Order status updated successfully" });
        }

        [HttpPost("{orderId}/create-payment")]
        public async Task<IActionResult> CreatePaymentOrder(int orderId)
        {
            var userId = GetUserId();

            // Verify the order belongs to the user
            var orderResult = await _orderService.GetOrderByIdAsync(userId, orderId);
            if (orderResult.Errors.Any())
            {
                return BadRequest(orderResult.Errors);
            }

            var order = orderResult.Response;
            var paymentResult = await _paymentService.CreateRazorpayOrderAsync(orderId, order.TotalAmount);

            if (paymentResult.Errors.Any())
            {
                return BadRequest(paymentResult.Errors);
            }

            return Ok(paymentResult.Response);
        }
        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerificationDto verificationDto)
        {
            var result = await _paymentService.VerifyPaymentAsync(verificationDto);

            if (result.Errors.Any())
            {
                // Payment verification failed
                await _paymentService.UpdateOrderPaymentStatusAsync(verificationDto.OrderId,
                    verificationDto.RazorpayPaymentId, false);
                return BadRequest(result.Errors);
            }

            // Payment verification successful
            return Ok(new { message = "Payment verified successfully", orderId = verificationDto.OrderId });
        }
    }
}
