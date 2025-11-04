using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EComApi.Controllers.Admin
{
    [Route("api/Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleService _roleService;

        public ProductsController(ApplicationDbContext context, IRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            // Optional: Verify admin role
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var product = new Products
            {
                Name = createProductDto.Name,
                Price = createProductDto.Price,
                Stock = createProductDto.Stock,
                Description = createProductDto.Description,
                Category = createProductDto.Category,
                Brand = createProductDto.Brand,
                Color = createProductDto.Color,
                ImageUrl = createProductDto.ImageUrl,
                DiscountPrice = createProductDto.DiscountPrice,
                Rating = createProductDto.Rating,
                HasGPS = createProductDto.HasGPS,
                HasHeartRate = createProductDto.HasHeartRate,
                HasSleepTracking = createProductDto.HasSleepTracking,
                HasBluetooth = createProductDto.HasBluetooth,
                HasWaterResistance = createProductDto.HasWaterResistance,
                HasNFC = createProductDto.HasNFC,
                // Server-set properties
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Ok(product);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = updateProductDto.Name;
            product.Price = updateProductDto.Price;
            product.Stock = updateProductDto.Stock;
            product.Description = updateProductDto.Description;
            product.Category = updateProductDto.Category;
            product.Brand = updateProductDto.Brand;
            product.Color = updateProductDto.Color;
            product.ImageUrl = updateProductDto.ImageUrl;
            product.DiscountPrice = updateProductDto.DiscountPrice;
            product.Rating = updateProductDto.Rating;
            product.HasGPS = updateProductDto.HasGPS;
            product.HasHeartRate = updateProductDto.HasHeartRate;
            product.HasSleepTracking = updateProductDto.HasSleepTracking;
            product.HasBluetooth = updateProductDto.HasBluetooth;
            product.HasWaterResistance = updateProductDto.HasWaterResistance;
            product.HasNFC = updateProductDto.HasNFC;

            await _context.SaveChangesAsync();

            return Ok(product);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!await IsCurrentUserAdmin())
            {
                return Forbid();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }

        private async Task<bool> IsCurrentUserAdmin()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return await _roleService.IsUserAdmin(userId);
        }
    }
    public class CreateProductDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal Rating { get; set; }
        public bool HasGPS { get; set; }
        public bool HasHeartRate { get; set; }
        public bool HasSleepTracking { get; set; }
        public bool HasBluetooth { get; set; }
        public bool HasWaterResistance { get; set; }
        public bool HasNFC { get; set; }
    }

    public class UpdateProductDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string ImageUrl { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal Rating { get; set; }
        public bool HasGPS { get; set; }
        public bool HasHeartRate { get; set; }
        public bool HasSleepTracking { get; set; }
        public bool HasBluetooth { get; set; }
        public bool HasWaterResistance { get; set; }
        public bool HasNFC { get; set; }
    }
}
