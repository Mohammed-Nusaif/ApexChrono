namespace EComApi.Entity.DTO.Cart
{
    public class ShoppingCartDto
    {
        public int CartId { get; set; }
        public string UserId { get; set; }

        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal TotalAmount { get; set; }

        public int TotalItems => Items.Sum(item => item.Quantity);

        // ✅ Optional: helpful for UI consistency
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
