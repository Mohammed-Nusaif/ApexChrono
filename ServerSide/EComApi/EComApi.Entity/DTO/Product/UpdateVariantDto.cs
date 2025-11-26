
using Microsoft.AspNetCore.Http;

namespace EComApi.Entity.DTO.Product
{
    public class UpdateVariantDto: CreateVariantDto
    {
        public int Id { get; set; }  // required
        public string? Color { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public List<string>? ImageUrls { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
