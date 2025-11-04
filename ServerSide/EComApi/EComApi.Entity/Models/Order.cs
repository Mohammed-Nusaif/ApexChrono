
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EComApi.Entity.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [Column(TypeName ="decimal(18,2)")]
        public Decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        // Razorpay fields 
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }
        // Shipping information
        public string ShippingAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        // Navigation property
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public enum OrderStatus
        {
            Pending,        // Order created, payment pending
            PaymentFailed,  // Payment failed
            Confirmed,      // Payment successful
            Processing,     // Order being processed
            Shipped,        // Order shipped
            Delivered,      // Order delivered
            Cancelled,      // Order cancelled
            Refunded        // Payment refunded
        }
    }
}
