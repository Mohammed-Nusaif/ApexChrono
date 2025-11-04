using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
using EComApi.Entity.Models;

namespace EComApi.Services.Services
{
    public  interface IPaymentService
    {
        Task<Result<RazorpayOrderResponseDto>> CreateRazorpayOrderAsync(int orderId, decimal amount);
        Task<Result<bool>> VerifyPaymentAsync(PaymentVerificationDto verificationDto);
        Task<Result<bool>> UpdateOrderPaymentStatusAsync(int orderId, string paymentId, bool isSuccess);

    }
}
