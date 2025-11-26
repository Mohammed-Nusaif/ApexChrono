namespace EComApi.Entity.DTO.Order
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public string ShippingAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        // ✅ Added: variant details
        public int? VariantId { get; set; }
        public string? VariantColor { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        // Optional — frontend display helper
        public string? DisplayImageUrl { get; set; }
    }
}
