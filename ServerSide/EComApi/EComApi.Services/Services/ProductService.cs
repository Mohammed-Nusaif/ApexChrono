using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO.Shared;
using EComApi.Entity.Models;
using Microsoft.EntityFrameworkCore;

namespace EComApi.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ FILTERED PRODUCTS (Variant-aware)
        public async Task<Result<List<Products>>> GetFilteredProductsAsync(ProductFilterDto filter)
        {
            var result = new Result<List<Products>>();
            try
            {
                var query = _context.Products
                    .Include(p => p.Variants)
                    .Where(p => p.IsActive)
                    .AsQueryable();

                // 🔹 Price filtering (use variant prices)
                if (filter.MinPrice.HasValue)
                    query = query.Where(p => p.Variants.Any(v => v.Price >= filter.MinPrice.Value));

                if (filter.MaxPrice.HasValue)
                    query = query.Where(p => p.Variants.Any(v => v.Price <= filter.MaxPrice.Value));

                // 🔹 Category & Brand filters
                if (!string.IsNullOrEmpty(filter.Category) && filter.Category != "string")
                    query = query.Where(p => p.Category == filter.Category);

                if (!string.IsNullOrEmpty(filter.Brand) && filter.Brand != "string")
                    query = query.Where(p => p.Brand == filter.Brand);

                // 🔹 Rating
                if (filter.MinRating.HasValue)
                    query = query.Where(p => p.Rating >= filter.MinRating.Value);

                // 🔹 Stock filter (any variant in stock)
                if (filter.InStockOnly)
                    query = query.Where(p => p.Variants.Any(v => v.Stock > 0));

                // 🔹 On Sale filter (DiscountPrice < BasePrice)
                if (filter.OnSaleOnly)
                    query = query.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice < p.BasePrice);

                // 🔹 Features
                if (filter.Features != null && filter.Features.Any() && !filter.Features.Contains("string"))
                {
                    if (filter.Features.Contains("GPS")) query = query.Where(p => p.HasGPS);
                    if (filter.Features.Contains("Heart Rate")) query = query.Where(p => p.HasHeartRate);
                    if (filter.Features.Contains("Sleep Tracking")) query = query.Where(p => p.HasSleepTracking);
                    if (filter.Features.Contains("Bluetooth")) query = query.Where(p => p.HasBluetooth);
                    if (filter.Features.Contains("Water Resistance")) query = query.Where(p => p.HasWaterResistance);
                    if (filter.Features.Contains("NFC")) query = query.Where(p => p.HasNFC);
                }

                // 🔹 Search term (in name or description)
                if (!string.IsNullOrEmpty(filter.SearchTerm) && filter.SearchTerm != "string")
                {
                    var term = filter.SearchTerm.ToLower();
                    query = query.Where(p =>
                        p.Name.ToLower().Contains(term) ||
                        p.Description.ToLower().Contains(term) ||
                        p.Brand.ToLower().Contains(term) ||
                        p.Category.ToLower().Contains(term));
                }

                result.Response = await query.ToListAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ GET CATEGORIES
        public async Task<Result<List<string>>> GetCategoriesAsync()
        {
            var result = new Result<List<string>>();
            try
            {
                result.Response = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ GET BRANDS
        public async Task<Result<List<string>>> GetBrandsAsync()
        {
            var result = new Result<List<string>>();
            try
            {
                result.Response = await _context.Products
                    .Where(p => p.IsActive)
                    .Select(p => p.Brand)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ GET ALL PRODUCTS (with variants)
        public async Task<Result<List<Products>>> GetAllProductsAsync()
        {
            var result = new Result<List<Products>>();
            try
            {
                result.Response = await _context.Products
                    .Include(p => p.Variants)
                    .Where(p => p.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ GET PRODUCT BY ID (with variants)
        public async Task<Result<Products>> GetProductByIdAsync(int id)
        {
            var result = new Result<Products>();
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                    result.Errors.Add(new Error { ErrorCode = 404, ErrorMessage = "Product not found or inactive" });
                else
                    result.Response = product;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ ADD PRODUCT (with variants)
        public async Task<Result<Products>> AddProductAsync(Products product)
        {
            var result = new Result<Products>();
            try
            {
                product.ImageUrls ??= new List<string>();
                product.CreatedDate = DateTime.UtcNow;
                product.IsActive = true;

                // Ensure variants have correct Product reference
                if (product.Variants != null && product.Variants.Count > 0)
                {
                    foreach (var variant in product.Variants)
                    {
                        variant.ImageUrls ??= new List<string>();
                        variant.IsActive = true;
                    }
                }

                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();

                result.Response = product;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ UPDATE PRODUCT
        public async Task<Result<Products>> UpdateProductAsync(Products product)
        {
            var result = new Result<Products>();
            try
            {
                var existing = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existing == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 404, ErrorMessage = "Product not found" });
                    return result;
                }

                // 🔹 Update base fields
                existing.Name = product.Name ?? existing.Name;
                existing.Brand = product.Brand ?? existing.Brand;
                existing.Category = product.Category ?? existing.Category;
                existing.Description = product.Description ?? existing.Description;
                existing.BasePrice = product.BasePrice != 0 ? product.BasePrice : existing.BasePrice;
                existing.DiscountPrice = product.DiscountPrice ?? existing.DiscountPrice;
                existing.Rating = product.Rating != 0 ? product.Rating : existing.Rating;
                existing.HasGPS = product.HasGPS;
                existing.HasHeartRate = product.HasHeartRate;
                existing.HasSleepTracking = product.HasSleepTracking;
                existing.HasBluetooth = product.HasBluetooth;
                existing.HasWaterResistance = product.HasWaterResistance;
                existing.HasNFC = product.HasNFC;
                existing.UpdatedDate = DateTime.UtcNow;

                // 🔹 Update images
                if (product.ImageUrls != null && product.ImageUrls.Any())
                    existing.ImageUrls = product.ImageUrls;

                // 🔹 Replace variants safely
                if (product.Variants != null && product.Variants.Any())
                {
                    _context.ProductVariants.RemoveRange(existing.Variants);
                    existing.Variants = product.Variants;
                }

                await _context.SaveChangesAsync();
                result.Response = existing;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        // ✅ DELETE PRODUCT
        public async Task<Result<bool>> DeleteProductAsync(int id)
        {
            var result = new Result<bool>();
            try
            {
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 404, ErrorMessage = "Product not found" });
                    return result;
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                result.Response = true;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }
    }
}
