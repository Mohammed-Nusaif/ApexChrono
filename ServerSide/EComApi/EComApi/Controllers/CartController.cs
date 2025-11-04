using EComApi.Entity.DTO;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EComApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication for all cart operations
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }
        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   User.FindFirstValue("sub"); // JWT sub claim
        }
        [HttpGet("GetCart")]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addToCartDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var result = await _cartService.AddToCartAsync(userId, addToCartDto);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }
        [HttpPut("update/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] int quantity)
        {
            var userId = GetUserId();
            var result = await _cartService.UpdateCartAsync(userId, cartItemId, quantity);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = GetUserId();
            var result = await _cartService.RemoveFromCartAsync(userId, cartItemId);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Response);
        }
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var result = await _cartService.ClearCartAsync(userId);

            if (result.Errors.Any())
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Cart cleared successfully" });
        }


    }
}
