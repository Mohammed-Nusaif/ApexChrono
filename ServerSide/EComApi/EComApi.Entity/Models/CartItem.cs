using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EComApi.Entity.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShoppingCartId { get; set; }

        [ForeignKey(nameof(ShoppingCartId))]
        public ShoppingCart ShoppingCart { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Products Products { get; set; }

        // ✅ Added: Variant support
        public int? VariantId { get; set; }

        [ForeignKey(nameof(VariantId))]
        public ProductVariant? ProductVariant { get; set; }

        [MaxLength(50)]
        public string? VariantColor { get; set; } // Snapshot color or type chosen

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // Captures price when added to cart

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity; // Computed in memory only

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
