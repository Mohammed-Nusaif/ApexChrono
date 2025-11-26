using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EComApi.Entity.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        // Base details
        public decimal BasePrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal Rating { get; set; } = 0;

        // 🔹 Main image (used for listing)
        public string? ThumbnailUrl { get; set; }

        // 🔹 Multiple gallery images
        public List<string>? ImageUrls { get; set; }

        // 🔹 Variants (e.g. color, strap type)
        public List<ProductVariant>? Variants { get; set; }

        // 🔹 Additional metadata
        public bool HasGPS { get; set; }
        public bool HasHeartRate { get; set; }
        public bool HasSleepTracking { get; set; }
        public bool HasBluetooth { get; set; }
        public bool HasWaterResistance { get; set; }
        public bool HasNFC { get; set; }

        // 🔹 Lifecycle
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // 🔹 Optional: Tags for search or filters
        public List<string>? Tags { get; set; }

        // 🔹 Optional: Average stock (useful for quick dashboard)
        [NotMapped]
        public int TotalStock => Variants?.Sum(v => v.Stock) ?? 0;

        public int Stock { get; set; }
    }
}
