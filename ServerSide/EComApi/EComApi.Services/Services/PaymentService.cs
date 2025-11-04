
using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
using EComApi.Entity.Models;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using static EComApi.Entity.Models.Order;

namespace EComApi.Services.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RazorpayClient _razorpayClient;

        public PaymentService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;

            var key = _configuration["Razorpay:Key"];
            var secret = _configuration["Razorpay:Secret"];

            Console.WriteLine($"Key: {key}");
            Console.WriteLine($"Secret: {secret}");

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(secret))
            {
                throw new Exception($"Razorpay credentials are missing. Key: '{key}', Secret: '{secret}'");
            }

            _razorpayClient = new RazorpayClient(key, secret);
        }

        public async Task<Result<RazorpayOrderResponseDto>> CreateRazorpayOrderAsync(int orderId, decimal amount)
        {
            var result = new Result<RazorpayOrderResponseDto>();
            try
            {
                // 1. Validate our order exists
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 401, ErrorMessage = "Order not found" });
                    return result;
                }
                // 2. Convert amount to paise (Razorpay expects amount in smallest currency unit)
                var amountInPaise = (int)(amount * 100);
                // 3. Create Razorpay order
                var options = new Dictionary<string, object>
                {
                    { "amount", amountInPaise },
                    { "currency", "INR" },
                    { "receipt", $"order_{orderId}" },
                    { "payment_capture", 1 } // Auto capture payment
                };
                Razorpay.Api.Order razorpayOrder = _razorpayClient.Order.Create(options);
                // 4. Update our order with Razorpay order ID
                order.RazorpayOrderId = razorpayOrder["id"].ToString();
                await _context.SaveChangesAsync();
                // 5. Prepare response
                var response = new RazorpayOrderResponseDto
                {
                    Id = razorpayOrder["id"].ToString(),
                    Amount = amount,
                    Currency = razorpayOrder["currency"].ToString(),
                    Receipt = razorpayOrder["receipt"].ToString(),
                    Status = razorpayOrder["status"].ToString(),
                    AmountDue = Convert.ToDecimal(razorpayOrder["amount_due"]) / 100,
                    AmountPaid = Convert.ToDecimal(razorpayOrder["amount_paid"]) / 100
                };

                result.Response = response;


            }
            catch (Exception ex)
            {

                result.Errors.Add(new Error { ErrorCode = 402, ErrorMessage = $"Error creating Razorpay order: {ex.Message}" });
            }
            return result;
        }

        public async Task<Result<bool>> VerifyPaymentAsync(PaymentVerificationDto verificationDto)
        {
            var result = new Result<bool>();
            try
            {
                var order = await _context.Orders.FindAsync(verificationDto.OrderId);
                if (order != null)
                {
                    result.Errors.Add(new Error { ErrorCode = 403, ErrorMessage = "Order not found" });
                }
                else
                {
                    return result;
                }
                // 2. Verify payment signature
                var attributes = new Dictionary<string, string>
                {
                    { "razorpay_payment_id", verificationDto.RazorpayPaymentId },
                    { "razorpay_order_id", verificationDto.RazorpayOrderId },
                    { "razorpay_signature", verificationDto.RazorpaySignature }
                };
                Utils.verifyPaymentSignature(attributes);
                // 3. If signature verification passes, update order status
                order.RazorpayPaymentId = verificationDto.RazorpayPaymentId;
                order.RazorpaySignature = verificationDto.RazorpaySignature;
                order.Status = OrderStatus.Confirmed;

                await _context.SaveChangesAsync();
                result.Response = true;
            }
            catch (Exception ex)
            {

                result.Errors.Add(new Error { ErrorCode = 404, ErrorMessage = $"Payment verification failed: {ex.Message}" });
                result.Response = false;
            }
            return result;
        }

        public async Task<Result<bool>> UpdateOrderPaymentStatusAsync(int orderId, string paymentId, bool isSuccess)
        {
            var result = new Result<bool>();
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 405, ErrorMessage = "Order not found" });
                    return result;
                }
                order.RazorpayPaymentId = paymentId;
                order.Status = isSuccess ? OrderStatus.Confirmed : OrderStatus.PaymentFailed;

                await _context.SaveChangesAsync();
                result.Response = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 406, ErrorMessage = $"Error updating payment status: {ex.Message}" });
            }
            return result;
        }
    }
}
