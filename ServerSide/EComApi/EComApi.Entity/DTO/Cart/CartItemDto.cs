namespace EComApi.Entity.DTO.Cart
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        // ✅ Variant support
        public int? VariantId { get; set; }
        public string? VariantColor { get; set; }

        public decimal? Price { get; set; }
        public int Quantity { get; set; }

        public decimal? TotalPrice => Price * Quantity;
        public int AvailableStock { get; set; }

        // Optional — image to show in cart (from variant or product)
        public string? DisplayImageUrl { get; set; }
    }
}
