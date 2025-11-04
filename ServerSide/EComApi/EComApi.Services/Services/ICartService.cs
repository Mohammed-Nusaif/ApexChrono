using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
using EComApi.Entity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EComApi.Services.Services
{
    public interface ICartService
    {
        Task<ShoppingCartDto> GetUserCartAsync(string userId);
        Task<Result<ShoppingCartDto>> AddToCartAsync(string userId,AddToCartDto addToCartDto);
        Task<Result<ShoppingCartDto>> UpdateCartAsync(string userId,int cartItemId, int quantity);
        Task<Result<ShoppingCartDto>> RemoveFromCartAsync(string userId,int cartItemId);
        Task<Result<bool>> ClearCartAsync(string userId);
        Task<ShoppingCart> GetOrCreateCartAsync(string userId);

    }
}
