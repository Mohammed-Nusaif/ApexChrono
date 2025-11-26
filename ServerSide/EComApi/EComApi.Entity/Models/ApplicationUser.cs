using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EComApi.Entity.Models
{
    public class ApplicationUser : IdentityUser
    {
        // ✅ Basic Info
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Country { get; set; }

        [MaxLength(10)]
        public string? PostalCode { get; set; }

        // ✅ Profile & Timestamps
        public string? ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        // ✅ Relations
        public virtual ICollection<Order>? Orders { get; set; }
        public virtual ICollection<ShoppingCart>? ShoppingCarts { get; set; }

        // ✅ Flags / Metadata
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;
    }
}
