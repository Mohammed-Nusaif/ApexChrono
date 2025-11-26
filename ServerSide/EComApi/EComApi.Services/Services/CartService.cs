using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO.Cart;
using EComApi.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace EComApi.Services.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ Get or create user cart
        public async Task<ShoppingCart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.ShoppingCart
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Products)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.ShoppingCart.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // ✅ Get user cart (DTO mapped)
        public async Task<ShoppingCartDto> GetUserCartAsync(string userId)
        {
            var cart = await _context.ShoppingCart
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Products)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart == null ? new ShoppingCartDto { UserId = userId } : MapToCartDto(cart);
        }

        // ✅ Add to cart (with variant logic)
        public async Task<Result<ShoppingCartDto>> AddToCartAsync(string userId, AddToCartDto addToCartDto)
        {
            var result = new Result<ShoppingCartDto>();

            var product = await _context.Products.FindAsync(addToCartDto.ProductId);
            if (product == null)
            {
                result.Errors.Add(new Error { ErrorCode = 201, ErrorMessage = "Product not found" });
                return result;
            }

            // ✅ If variant provided, validate it
            ProductVariant? variant = null;
            if (addToCartDto.VariantId.HasValue)
            {
                variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == addToCartDto.VariantId && v.ProductId == product.Id);

                if (variant == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 202, ErrorMessage = "Variant not found for this product" });
                    return result;
                }

                if (variant.Stock < addToCartDto.Quantity)
                {
                    result.Errors.Add(new Error { ErrorCode = 203, ErrorMessage = "Insufficient stock for this variant" });
                    return result;
                }
            }
            else if (product.Stock < addToCartDto.Quantity)
            {
                result.Errors.Add(new Error { ErrorCode = 204, ErrorMessage = "Insufficient stock available" });
                return result;
            }

            var cart = await GetOrCreateCartAsync(userId);

            // ✅ Identify same product + same variant as same line item
            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == addToCartDto.ProductId &&
                ci.VariantId == addToCartDto.VariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += addToCartDto.Quantity;
                existingItem.ShoppingCart.UpdateTimestamps();
            }
            else
            {
                var priceToUse = variant?.Price ?? product.BasePrice;

                var cartItem = new CartItem
                {
                    ShoppingCartId = cart.Id,
                    ProductId = addToCartDto.ProductId,
                    VariantId = addToCartDto.VariantId,
                    VariantColor = variant?.Color,
                    Quantity = addToCartDto.Quantity,
                    UnitPrice = priceToUse,
                    AddedAt = DateTime.UtcNow
                };

                _context.CartItems.Add(cartItem);
                cart.UpdateTimestamps();
            }

            await _context.SaveChangesAsync();

            result.Response = await GetUserCartAsync(userId);
            return result;
        }

        // ✅ Update cart item quantity
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
                .Include(ci => ci.ProductVariant)
                .Include(ci => ci.ShoppingCart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.ShoppingCart.UserId == userId);

            if (cartItem == null)
            {
                result.Errors.Add(new Error { ErrorCode = 204, ErrorMessage = "Cart item not found" });
                return result;
            }

            // Validate stock based on variant or product
            var availableStock = cartItem.ProductVariant?.Stock ?? cartItem.Products.Stock;
            if (availableStock < quantity)
            {
                result.Errors.Add(new Error { ErrorCode = 205, ErrorMessage = "Insufficient stock available" });
                return result;
            }

            cartItem.Quantity = quantity;
            cartItem.ShoppingCart.UpdateTimestamps();

            await _context.SaveChangesAsync();
            result.Response = await GetUserCartAsync(userId);
            return result;
        }

        // ✅ Remove from cart
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
            cartItem.ShoppingCart.UpdateTimestamps();

            await _context.SaveChangesAsync();
            result.Response = await GetUserCartAsync(userId);
            return result;
        }

        // ✅ Clear entire cart
        public async Task<Result<bool>> ClearCartAsync(string userId)
        {
            var result = new Result<bool>();
            var cart = await _context.ShoppingCart
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdateTimestamps();
                await _context.SaveChangesAsync();
            }

            result.Response = true;
            return result;
        }

        // ✅ DTO Mapper
        private ShoppingCartDto MapToCartDto(ShoppingCart cart)
        {
            return new ShoppingCartDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Products.Name,
                    VariantId = ci.VariantId,
                    VariantColor = ci.VariantColor,
                    Price = ci.UnitPrice,
                    Quantity = ci.Quantity,
                    AvailableStock = ci.ProductVariant?.Stock ?? ci.Products.Stock
                }).ToList(),
                TotalAmount = cart.TotalAmount
            };
        }
    }
}
