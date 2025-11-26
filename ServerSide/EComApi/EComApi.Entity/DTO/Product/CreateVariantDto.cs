
using Microsoft.AspNetCore.Http;

namespace EComApi.Entity.DTO.Product
{
    public class CreateVariantDto
    {
        public string? Color { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
