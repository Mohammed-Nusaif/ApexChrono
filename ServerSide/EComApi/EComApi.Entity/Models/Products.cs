
using System.ComponentModel.DataAnnotations;

namespace EComApi.Entity.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock {  get; set; }
        public string Description { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(30)]
        public string Brand { get; set; }

        [StringLength(30)]
        public string Color { get; set; }

        [StringLength(500)]
        public string ImageUrl { get; set; }

        public decimal? DiscountPrice { get; set; } // For "On Sale" filter
        public decimal Rating { get; set; } = 0; // For customer rating filter
        public bool IsActive { get; set; } = true;

        // Features for "Features" filter
        public bool HasGPS { get; set; }
        public bool HasHeartRate { get; set; }
        public bool HasSleepTracking { get; set; }
        public bool HasBluetooth { get; set; }
        public bool HasWaterResistance { get; set; }
        public bool HasNFC { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
