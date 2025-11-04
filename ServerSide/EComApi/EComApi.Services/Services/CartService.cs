using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
using EComApi.Entity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EComApi.Services.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.ShoppingCart
                        .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Products)
                        .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                _context.ShoppingCart.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }
        public async Task<ShoppingCartDto> GetUserCartAsync(string userId)
        {
            var cart = await _context.ShoppingCart
                       .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Products)
                        .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) {
                return new ShoppingCartDto { UserId = userId };
            }
            return MapToCartDto(cart);
        }


        public async Task<Result<ShoppingCartDto>> AddToCartAsync(string userId, AddToCartDto addToCartDto)
        {
            var result = new Result<ShoppingCartDto>();
            // Validate product exists and has stock
            var product = await _context.Products.FindAsync(addToCartDto.ProductId);
            if (product == null) {
                result.Errors.Add(new Error { ErrorCode = 201, ErrorMessage = "Product not found" });
                return result;
            }
            if (product.Stock < addToCartDto.Quantity)
            {
                result.Errors.Add(new Error { ErrorCode = 202, ErrorMessage = "Insufficient stock available" });
                return result;
            }
            var cart = await GetOrCreateCartAsync(userId);
            // Check if item already exists in cart
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addToCartDto.ProductId);
            if (existingItem != null) {
                // Update quantity if item exists
                existingItem.Quantity += addToCartDto.Quantity;
            }
            else
            {
                // Add new item to cart
                var cartItem = new CartItem
                {
                    ShoppingCartId = cart.Id,
                    ProductId = addToCartDto.ProductId,
                    Quantity = addToCartDto.Quantity,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(cartItem);
            }
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Return updated cart
            var updatedCart = await GetUserCartAsync(userId);
            result.Response = updatedCart;
            return result;
        }
        public async Task<Result<ShoppingCartDto>> UpdateCartAsync(string userId, int cartItemId, int quantity)
        {
            var result = new Result<ShoppingCartDto>();

            if (quantity < 1)
            {
                result.Errors.Add(new Error { ErrorCode = 203, ErrorMessage = "Quantity must be at least 1" });
                return result;
            }
            var cartItem = await _context.CartItems
                .Include(ci => ci.Products)
                .Include(ci => ci.ShoppingCart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.ShoppingCart.UserId == userId);
            if (cartItem == null)
            {
                result.Errors.Add(new Error { ErrorCode = 204, ErrorMessage = "Cart item not found" });
                return result;
            }
            if (cartItem.Products.Stock < quantity)
            {
                result.Errors.Add(new Error { ErrorCode = 202, ErrorMessage = "Insufficient stock available" });
                return result;
            }
            cartItem.Quantity = quantity;
            cartItem.ShoppingCart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedCart = await GetUserCartAsync(userId);
            result.Response = updatedCart;
            return result;
        }
        public async Task<Result<ShoppingCartDto>> RemoveFromCartAsync(string userId, int cartItemId)
        {
            var result = new Result<ShoppingCartDto>();
            var cartItem = await _context.CartItems
              .Include(ci => ci.ShoppingCart)
              .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.ShoppingCart.UserId == userId);
            if (cartItem == null)
            {
                result.Errors.Add(new Error { ErrorCode = 204, ErrorMessage = "Cart item not found" });
                return result;
            }
            _context.CartItems.Remove(cartItem);

            cartItem.ShoppingCart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var updatedCart = await GetUserCartAsync(userId);
            result.Response = updatedCart;
            return result;

        }

        public async Task<Result<bool>> ClearCartAsync(string userId)
        {
            var result = new Result<bool>();
            var cart = await _context.ShoppingCart
               .Include(c => c.CartItems)
               .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            result.Response = true;
            return result;

        }
        private ShoppingCartDto MapToCartDto(ShoppingCart cart)
        {
            return new ShoppingCartDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    ProductId = ci.Products.Id,
                    ProductName = ci.Products.Name,
                    Price = ci.Products.Price,
                    Quantity = ci.Quantity,
                    AvailableStock = ci.Products.Stock
                }).ToList(),
                TotalAmount = cart.TotalAmount
            };
        }




    }
}
