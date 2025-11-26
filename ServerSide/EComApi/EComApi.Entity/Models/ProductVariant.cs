using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace EComApi.Entity.Models
{
    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(ProductId))]
        public Products Product { get; set; }

        // Variant details
        [Required]
        [MaxLength(50)]
        public string Color { get; set; }

        [MaxLength(50)]
        public string? Material { get; set; }  // optional future field (e.g., "Aluminium", "Stainless Steel")

        [MaxLength(50)]
        public string? Size { get; set; }  // optional future field (e.g., "41mm", "45mm")

        public decimal Price { get; set; }
        public int Stock { get; set; }
        public List<string>? ImageUrls { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
