
namespace EComApi.Entity.DTO
{
    public class ProductFilterDto
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public decimal? MinRating { get; set; }
        public bool InStockOnly { get; set; }
        public bool OnSaleOnly { get; set; }
        public List<string> Features { get; set; } = new List<string>();
        public string? SearchTerm { get; set; }
    }
}
