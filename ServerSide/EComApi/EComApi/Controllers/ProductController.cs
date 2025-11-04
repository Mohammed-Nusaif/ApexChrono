using EComApi.Entity.DTO;
using EComApi.Entity.Models;
using EComApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        [Authorize]
        [HttpGet("GetALlProduct")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.GetAllProductsAsync();
            if (result.isError) return BadRequest(result.Errors);
            return Ok(result.Response);
        }

        [Authorize]
        [HttpGet("GetProductById{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (result.isError) return NotFound(result.Errors);
            return Ok(result.Response);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct(Products product)
        {
            var result = await _productService.AddProductAsync(product);
            if (result.isError) return BadRequest(result.Errors);
            return Ok(result.Response);
        }

        [HttpPut("UpdateProduct{id}")]
        public async Task<IActionResult> Update(int id, Products product)
        {
            if (id != product.Id) return BadRequest("ID mismatch");

            var result = await _productService.UpdateProductAsync(product);
            if (result.isError) return BadRequest(result.Errors);
            return Ok(result.Response);
        }

        [HttpDelete("DeleteProduct{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result.isError) return NotFound(result.Errors);
            return Ok(result.Response);
        }

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
            return Ok(result.Response);
        }
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _productService.GetCategoriesAsync();
            if (result.Errors.Any())
                return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("brands")]
        public async Task<IActionResult> GetBrands()
        {
            var result = await _productService.GetBrandsAsync();
            if (result.Errors.Any())
                return BadRequest(result);
            return Ok(result);
        }
    }
}
