namespace EComApi.Entity.DTO.Payment
{
    public class RazorpayOrderRequestDto
    {
        public int OrderId { get; set; } // Our system order ID
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Receipt { get; set; }
    }
}
