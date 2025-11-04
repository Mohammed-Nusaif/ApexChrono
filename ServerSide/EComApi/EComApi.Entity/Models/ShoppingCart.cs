
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
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime CreatedAt {  get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Calculated property for total amount
        [NotMapped]
        public decimal TotalAmount
        {
            get
            {
                return CartItems?.Sum(item => item.Quantity * item.Products.Price) ?? 0;
            }
        }
    }
}
