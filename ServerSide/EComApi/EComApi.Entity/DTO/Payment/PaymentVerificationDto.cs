namespace EComApi.Entity.DTO.Payment
{
    public class PaymentVerificationDto
    {
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
        public int OrderId { get; set; } // Our system order ID
    }
}
