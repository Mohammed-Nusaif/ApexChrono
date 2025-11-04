
using System.ComponentModel.DataAnnotations;

namespace EComApi.Entity.DTO
{
    public class CreateOrderDto
    {
        [Required]
        public string ShippingAddress { get; set; }

        [Required]
        [Phone]
        public string CustomerPhone { get; set; }

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; }
    }
}
