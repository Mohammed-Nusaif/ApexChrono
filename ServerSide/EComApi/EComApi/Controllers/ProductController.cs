using EComApi.Entity.DTO.Shared;
using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EComApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ✅ GET ALL PRODUCTS (with variants)
        [Authorize]
        [HttpGet("GetALlProduct")]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await _productService.GetAllProductsAsync();
            if (result.isError) return BadRequest(result.Errors);
            return Ok(new { count = result.Response.Count, products = result.Response });
        }

        // ✅ GET PRODUCT BY ID (with variants)
        [Authorize]
        [HttpGet("GetProductById/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (result.isError) return NotFound(result.Errors);
            return Ok(result.Response);
        }

        // ✅ ADD PRODUCT (with variants + images)
        [HttpPost("AddProduct")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] Products product)
        {
            if (product == null)
                return BadRequest("Product data is required");

            var result = await _productService.AddProductAsync(product);
            if (result.isError) return BadRequest(result.Errors);
            return Ok(new { message = "Product created successfully", data = result.Response });
        }

        // ✅ UPDATE PRODUCT (with variants)
        [HttpPut("UpdateProduct/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Products product)
        {
            if (id != product.Id)
                return BadRequest("Product ID mismatch");

            var result = await _productService.UpdateProductAsync(product);
            if (result.isError) return BadRequest(result.Errors);
            return Ok(new { message = "Product updated successfully", data = result.Response });
        }

        // ✅ DELETE PRODUCT
        [HttpDelete("DeleteProduct/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result.isError) return NotFound(result.Errors);
            return Ok(new { message = "Product deleted successfully" });
        }

        // ✅ FILTERED PRODUCTS
        [HttpPost("filter")]
        public async Task<IActionResult> GetFilteredProducts([FromBody] ProductFilterDto filter)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { errors });
            }

            var result = await _productService.GetFilteredProductsAsync(filter);
            if (result.isError) return BadRequest(result.Errors);
            return Ok(new { count = result.Response.Count, products = result.Response });
        }

        // ✅ GET CATEGORIES
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _productService.GetCategoriesAsync();
            if (result.isError) return BadRequest(result.Errors);
            return Ok(result.Response);
        }

        // ✅ GET BRANDS
        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var result = await _productService.GetBrandsAsync();
            if (result.isError) return BadRequest(result.Errors);
            return Ok(result.Response);
        }

        // ✅ NEW: GET VARIANTS FOR SPECIFIC PRODUCT
        [HttpGet("{productId}/variants")]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            var result = await _productService.GetProductByIdAsync(productId);
            if (result.isError) return NotFound(result.Errors);

            var product = result.Response;
            if (product?.Variants == null || !product.Variants.Any())
                return Ok(new { message = "No variants available for this product", variants = new List<ProductVariant>() });

            return Ok(new { productId = product.Id, variants = product.Variants });
        }
    }
}
