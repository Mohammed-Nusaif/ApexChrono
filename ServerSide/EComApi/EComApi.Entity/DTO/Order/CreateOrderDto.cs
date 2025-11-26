using System.ComponentModel.DataAnnotations;

namespace EComApi.Entity.DTO.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        [Phone]
        public string CustomerPhone { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }

        // ✅ Optional fields for future scalability
        public string? PaymentMethod { get; set; } = "Razorpay";
        public string? OrderNotes { get; set; }
    }
}
