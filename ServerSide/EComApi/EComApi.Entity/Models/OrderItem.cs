using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EComApi.Entity.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public Order Order { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Products Product { get; set; }

        // 🔹 Optional link to specific variant (color, size, etc.)
        public int? ProductVariantId { get; set; }

        [ForeignKey(nameof(ProductVariantId))]
        public ProductVariant? ProductVariant { get; set; }

        [MaxLength(50)]
        public string? VariantColor { get; set; } // Snapshot of variant color at purchase

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Price at time of order

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity; // Computed at runtime

        [Required]
        [MaxLength(255)]
        public string ProductName { get; set; } // Snapshot of product name
    }
}
