using EComApi.Entity.DTO.Product;
using EComApi.Entity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace EComApi.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ---------------------------------------------------------
        // 🔥 Helper: Convert relative URL → absolute URL
        // ---------------------------------------------------------
        private string ToAbsolute(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            // Already absolute (http / https) -> leave as is
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return url;

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            // url is like "/uploads/products/xxx.ext"
            return $"{baseUrl}{url}";
        }

        // ---------------------------------------------------------
        // 🔥 Normalize ALL product + variant image URLs
        // ---------------------------------------------------------
        private void FixImageUrls(Products product)
        {
            if (product == null)
                return;

            // Thumbnail
            product.ThumbnailUrl = ToAbsolute(product.ThumbnailUrl);

            // Product main image URLs
            if (product.ImageUrls != null && product.ImageUrls.Any())
            {
                product.ImageUrls = product.ImageUrls
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(ToAbsolute)           // convert all to absolute
                    .Distinct()                   // remove duplicates (relative + absolute of same file)
                    .ToList();
            }

            // Variants
            if (product.Variants != null && product.Variants.Any())
            {
                foreach (var v in product.Variants)
                {
                    if (v.ImageUrls != null && v.ImageUrls.Any())
                    {
                        v.ImageUrls = v.ImageUrls
                            .Where(u => !string.IsNullOrWhiteSpace(u))
                            .Select(ToAbsolute)
                            .Distinct()
                            .ToList();
                    }
                }
            }
        }

        // ---------------------------------------------------------
        // 1. GET ALL PRODUCTS (with variants)
        // ---------------------------------------------------------
        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Variants)
                .ToListAsync();

            // Fix URLs for every product
            foreach (var product in products)
                FixImageUrls(product);

            return Ok(products);
        }

        // ---------------------------------------------------------
        // 2. GET PRODUCT BY ID (with variants)
        // ---------------------------------------------------------
        [HttpGet("get-product-by-id/{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { success = false, message = "Product not found" });

            // Fix URLs
            FixImageUrls(product);

            return Ok(product);
        }

        // ---------------------------------------------------------
        // 3. CREATE PRODUCT
        // ---------------------------------------------------------
        [HttpPost("create-product")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto dto)
        {
            var imageUrls = dto.ImageUrls ?? new List<string>();

            // Uploaded files → /uploads/products/
            if (dto.Images != null && dto.Images.Any())
                imageUrls.AddRange(await SaveFilesAsync(dto.Images));

            var product = new Products
            {
                Name = dto.Name,
                BasePrice = dto.BasePrice ?? 0m,
                DiscountPrice = dto.DiscountPrice ?? 0m,
                Stock = dto.Stock ?? 0,
                Description = dto.Description,
                Category = dto.Category,
                Brand = dto.Brand,
                ImageUrls = imageUrls,
                ThumbnailUrl = imageUrls.FirstOrDefault(),
                Rating = dto.Rating ?? 0,
                HasGPS = dto.HasGPS ?? false,
                HasHeartRate = dto.HasHeartRate ?? false,
                HasSleepTracking = dto.HasSleepTracking ?? false,
                HasBluetooth = dto.HasBluetooth ?? false,
                HasWaterResistance = dto.HasWaterResistance ?? false,
                HasNFC = dto.HasNFC ?? false,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            // Variants
            if (dto.Variants != null && dto.Variants.Any())
            {
                product.Variants = new List<ProductVariant>();

                foreach (var variantDto in dto.Variants)
                {
                    var variant = new ProductVariant
                    {
                        Color = variantDto.Color,
                        Stock = variantDto.Stock ?? 0,
                        Price = variantDto.Price ?? product.BasePrice,
                        ImageUrls = variantDto.ImageUrls ?? new List<string>()
                    };

                    if (variantDto.Images != null && variantDto.Images.Any())
                        variant.ImageUrls.AddRange(await SaveFilesAsync(variantDto.Images));

                    product.Variants.Add(variant);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Fix URLs before returning
            FixImageUrls(product);

            return Ok(new { success = true, message = "Product created successfully", data = product });
        }

        // ---------------------------------------------------------
        // 4. UPDATE PRODUCT
        // ---------------------------------------------------------
        [HttpPut("update-product-by-id/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { success = false, message = "Product not found" });

            // Merge existing + new image URLs
            var imageUrls = product.ImageUrls ?? new List<string>();
            if (dto.ImageUrls != null) imageUrls.AddRange(dto.ImageUrls);
            if (dto.Images != null && dto.Images.Any())
                imageUrls.AddRange(await SaveFilesAsync(dto.Images));

            product.Name = dto.Name ?? product.Name;
            product.BasePrice = dto.BasePrice ?? product.BasePrice;
            product.Stock = dto.Stock ?? product.Stock;
            product.Description = dto.Description ?? product.Description;
            product.Category = dto.Category ?? product.Category;
            product.Brand = dto.Brand ?? product.Brand;
            product.DiscountPrice = dto.DiscountPrice ?? product.DiscountPrice;
            product.Rating = dto.Rating ?? product.Rating;
            product.HasGPS = dto.HasGPS ?? product.HasGPS;
            product.HasHeartRate = dto.HasHeartRate ?? product.HasHeartRate;
            product.HasSleepTracking = dto.HasSleepTracking ?? product.HasSleepTracking;
            product.HasBluetooth = dto.HasBluetooth ?? product.HasBluetooth;
            product.HasWaterResistance = dto.HasWaterResistance ?? product.HasWaterResistance;
            product.HasNFC = dto.HasNFC ?? product.HasNFC;
            product.ImageUrls = imageUrls.Distinct().ToList();
            product.ThumbnailUrl ??= imageUrls.FirstOrDefault();
            product.UpdatedDate = DateTime.UtcNow;

            // Update / Add variants
            if (dto.Variants != null && dto.Variants.Any())
            {
                foreach (var variantDto in dto.Variants)
                {
                    var existingVariant = product.Variants.FirstOrDefault(v => v.Id == variantDto.Id);

                    if (existingVariant != null)
                    {
                        existingVariant.Color = variantDto.Color ?? existingVariant.Color;
                        existingVariant.Price = variantDto.Price ?? existingVariant.Price;
                        existingVariant.Stock = variantDto.Stock ?? existingVariant.Stock;

                        existingVariant.ImageUrls ??= new List<string>(); // 👈 avoid null

                        if (variantDto.ImageUrls != null)
                            existingVariant.ImageUrls.AddRange(variantDto.ImageUrls);

                        if (variantDto.Images != null && variantDto.Images.Any())
                            existingVariant.ImageUrls.AddRange(await SaveFilesAsync(variantDto.Images));

                        if (existingVariant.ImageUrls != null)
                        {
                            existingVariant.ImageUrls = existingVariant.ImageUrls
                                .Where(u => !string.IsNullOrWhiteSpace(u))
                                .Distinct()
                                .ToList();
                        }
                    }
                    else
                    {
                        var newVariant = new ProductVariant
                        {
                            Color = variantDto.Color,
                            Price = variantDto.Price ?? product.BasePrice,
                            Stock = variantDto.Stock ?? 0,
                            ImageUrls = variantDto.ImageUrls ?? new List<string>()
                        };

                        if (variantDto.Images != null && variantDto.Images.Any())
                            newVariant.ImageUrls.AddRange(await SaveFilesAsync(variantDto.Images));

                        if (newVariant.ImageUrls != null)
                        {
                            newVariant.ImageUrls = newVariant.ImageUrls
                                .Where(u => !string.IsNullOrWhiteSpace(u))
                                .Distinct()
                                .ToList();
                        }

                        product.Variants.Add(newVariant);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Fix URLs before returning
            FixImageUrls(product);

            return Ok(new { success = true, message = "Product updated successfully", data = product });
        }

        // ---------------------------------------------------------
        // 5. Toggle product status
        // ---------------------------------------------------------
        [HttpPatch("toggle-status-by-id/{id}")]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { success = false, message = "Product not found" });

            product.IsActive = !product.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Product '{product.Name}' is now {(product.IsActive ? "Active" : "Inactive")}.",
                newStatus = product.IsActive
            });
        }

        // ---------------------------------------------------------
        // 6. DELETE PRODUCT
        // ---------------------------------------------------------
        [HttpDelete("delete-product-by-id/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound(new { success = false, message = "Product not found" });

            _context.ProductVariants.RemoveRange(product.Variants);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = $"Product '{product.Name}' deleted successfully" });
        }

        // ---------------------------------------------------------
        // Helper: Save uploaded files
        // ---------------------------------------------------------
        private async Task<List<string>> SaveFilesAsync(IEnumerable<IFormFile> files)
        {
            var urls = new List<string>();
            var uploadDir = Path.Combine(
                _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "uploads", "products"
            );

            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            foreach (var file in files)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadDir, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                // Store as relative path in DB, will be converted to absolute by FixImageUrls
                urls.Add($"/uploads/products/{fileName}");
            }

            return urls;
        }
    }
}
