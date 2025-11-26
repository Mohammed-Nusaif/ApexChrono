namespace EComApi.Entity.DTO.Order
{
    public class AdminOrderDto
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string CustomerPhone { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }

        public List<AdminOrderItemDto> OrderItems { get; set; } = new();
    }
}
