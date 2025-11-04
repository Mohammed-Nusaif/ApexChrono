namespace EComApi.Entity.DTO
{
    public class RazorpayOrderResponseDto
    {
        public string Id { get; set; } // Razorpay order ID
        public string Entity { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }
        public string Currency { get; set; }
        public string Receipt { get; set; }
        public string Status { get; set; }
        public int CreatedAt { get; set; }
    }
}
