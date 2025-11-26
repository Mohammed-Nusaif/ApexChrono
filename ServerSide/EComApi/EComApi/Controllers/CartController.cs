using EComApi.Entity.DTO.Cart;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EComApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                         User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            return userId;
        }

        [HttpGet("get-cart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(new { success = true, data = cart });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var userId = GetUserId();

            var result = await _cartService.AddToCartAsync(userId, addToCartDto);

            if (result.Errors.Any())
                return BadRequest(new { success = false, errors = result.Errors });

            return Ok(new { success = true, data = result.Response });
        }

        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] int quantity)
        {
            if (quantity <= 0)
                return BadRequest(new { success = false, error = "Quantity must be greater than zero." });

            var userId = GetUserId();
            var result = await _cartService.UpdateCartAsync(userId, cartItemId, quantity);

            if (result.Errors.Any())
                return BadRequest(new { success = false, errors = result.Errors });

            return Ok(new { success = true, data = result.Response });
        }

        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (result.Errors.Any())
                return BadRequest(new { success = false, errors = result.Errors });

            return Ok(new { success = true, data = result.Response });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId);

            if (result.Errors.Any())
                return BadRequest(new { success = false, errors = result.Errors });

            return Ok(new { success = true, message = "Cart cleared successfully" });
        }
    }
}
