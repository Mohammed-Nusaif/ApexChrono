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

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // 🧾 Payment Information
        [MaxLength(100)]
        public string? RazorpayOrderId { get; set; }

        [MaxLength(100)]
        public string? RazorpayPaymentId { get; set; }

        [MaxLength(256)]
        public string? RazorpaySignature { get; set; }

        // 🚚 Shipping Information
        [Required, MaxLength(500)]
        public string ShippingAddress { get; set; }

        [Required, MaxLength(15)]
        public string CustomerPhone { get; set; }

        [Required, MaxLength(100)]
        public string CustomerEmail { get; set; }

        // 💬 Optional fields for future expansion
        [MaxLength(255)]
        public string? AdminComment { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ShippedDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DeliveredDate { get; set; }

        // 🧩 Navigation property
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // 📦 Enum for tracking order lifecycle
        public enum OrderStatus
        {
            Pending,        // Order created, payment pending
            PaymentFailed,  // Payment failed
            Confirmed,      // Payment successful
            Processing,     // Preparing shipment
            Shipped,        // Dispatched
            Delivered,      // Customer received order
            Cancelled,      // Cancelled by admin or user
            Refunded        // Payment refunded
        }
    }
}
