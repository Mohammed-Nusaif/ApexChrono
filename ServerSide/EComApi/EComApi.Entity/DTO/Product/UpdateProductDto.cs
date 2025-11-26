using Microsoft.AspNetCore.Http;

namespace EComApi.Entity.DTO.Product
{
    public class UpdateProductDto
    {
        public int Id { get; set; }   // required for update

        public string? Name { get; set; }
        public decimal? BasePrice { get; set; }
        public int? Stock { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }

        public List<string>? ImageUrls { get; set; }
        public List<IFormFile>? Images { get; set; }

        public decimal? DiscountPrice { get; set; }
        public decimal? Rating { get; set; }

        public bool? HasGPS { get; set; }
        public bool? HasHeartRate { get; set; }
        public bool? HasSleepTracking { get; set; }
        public bool? HasBluetooth { get; set; }
        public bool? HasWaterResistance { get; set; }
        public bool? HasNFC { get; set; }

        public List<UpdateVariantDto>? Variants { get; set; }
    }
}
