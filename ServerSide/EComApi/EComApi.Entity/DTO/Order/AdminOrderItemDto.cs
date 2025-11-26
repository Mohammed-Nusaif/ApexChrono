namespace EComApi.Entity.DTO.Order
{
    public class AdminOrderItemDto
    {
        public string ProductName { get; set; }
        public string? VariantColor { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ImageUrl { get; set; }
    }
}
