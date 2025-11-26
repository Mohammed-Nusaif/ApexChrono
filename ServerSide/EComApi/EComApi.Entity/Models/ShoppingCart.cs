using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EComApi.Entity.Models
{
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation property
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // ✅ Computed property for total amount
        [NotMapped]
        public decimal TotalAmount =>
            CartItems?.Sum(item => item.Quantity * item.UnitPrice) ?? 0;

        // ✅ Optional helper for updating timestamps automatically
        public void UpdateTimestamps()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
