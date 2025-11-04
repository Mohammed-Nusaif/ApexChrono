using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EComApi.Entity.DTO
{
    public class ShoppingCartDto
    {
        public int CartId { get; set; }
        public string UserId { get; set; }
        public List<CartItemDto> Items { get; set; } =  new List<CartItemDto>();
        public decimal TotalAmount { get; set; }
        public int TotalItems => Items.Sum(item => item.Quantity);
    }
}
