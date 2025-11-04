using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO;
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
        public async Task<Result<List<Products>>> GetFilteredProductsAsync(ProductFilterDto filter)
        {
            var result = new Result<List<Products>>();
            try
            {
                var query = _context.Products.AsQueryable();

                // Price Range Filter
                if (filter.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= filter.MaxPrice.Value);

                // Category Filter
                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(p => p.Category == filter.Category);

                // Brand Filter
                if (!string.IsNullOrEmpty(filter.Brand))
                    query = query.Where(p => p.Brand == filter.Brand);

                // Rating Filter
                if (filter.MinRating.HasValue)
                    query = query.Where(p => p.Rating >= filter.MinRating.Value);

                // Availability Filter
                if (filter.InStockOnly)
                    query = query.Where(p => p.Stock > 0);

                // On Sale Filter
                if (filter.OnSaleOnly)
                    query = query.Where(p => p.DiscountPrice.HasValue && p.DiscountPrice < p.Price);

                // Features Filter
                if (filter.Features != null && filter.Features.Any())
                {
                    if (filter.Features.Contains("GPS"))
                        query = query.Where(p => p.HasGPS);
                    if (filter.Features.Contains("Heart Rate"))
                        query = query.Where(p => p.HasHeartRate);
                    if (filter.Features.Contains("Sleep Tracking"))
                        query = query.Where(p => p.HasSleepTracking);
                    if (filter.Features.Contains("Bluetooth"))
                        query = query.Where(p => p.HasBluetooth);
                    if (filter.Features.Contains("Water Resistance"))
                        query = query.Where(p => p.HasWaterResistance);
                    if (filter.Features.Contains("NFC"))
                        query = query.Where(p => p.HasNFC);
                }

                // Search by Name
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                    query = query.Where(p => p.Name.Contains(filter.SearchTerm) ||
                                           p.Description.Contains(filter.SearchTerm));

                result.Response = await query.ToListAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }
        public async Task<Result<List<string>>> GetCategoriesAsync()
        {
            var result = new Result<List<string>>();
            try
            {
                result.Response = await _context.Products
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

        public async Task<Result<List<string>>> GetBrandsAsync()
        {
            var result = new Result<List<string>>();
            try
            {
                result.Response = await _context.Products
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
        public async Task<Result<List<Products>>> GetAllProductsAsync()
        {
            var result = new Result<List<Products>>();
            try
            {
                result.Response = await _context.Products.ToListAsync();
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        public async Task<Result<Products>> GetProductByIdAsync(int id)
        {
            var result = new Result<Products>();
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 404, ErrorMessage = "Product not found" });
                }
                else
                {
                    result.Response = product;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        public async Task<Result<Products>> AddProductAsync(Products product)
        {
            var result = new Result<Products>();
            try
            {
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

        public async Task<Result<Products>> UpdateProductAsync(Products product)
        {
            var result = new Result<Products>();
            try
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                result.Response = product;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }

        public async Task<Result<bool>> DeleteProductAsync(int id)
        {
            var result = new Result<bool>();
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    result.Errors.Add(new Error { ErrorCode = 404, ErrorMessage = "Product not found" });
                }
                else
                {
                    _context.Products.Remove(product);
                    await _context.SaveChangesAsync();
                    result.Response = true;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(new Error { ErrorCode = 500, ErrorMessage = ex.Message });
            }
            return result;
        }
    }
}
