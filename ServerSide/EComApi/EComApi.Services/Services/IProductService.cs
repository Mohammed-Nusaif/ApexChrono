using EComApi.Common.Common.DTO;
using EComApi.Entity.DTO.Shared;
using EComApi.Entity.Models;


namespace EComApi.Services.Services
{
    public interface IProductService
    {
        Task<Result<List<Products>>> GetAllProductsAsync();
        Task<Result<Products>> GetProductByIdAsync(int id);
        Task<Result<Products>> AddProductAsync(Products product);
        Task<Result<Products>> UpdateProductAsync(Products product);
        Task<Result<bool>> DeleteProductAsync(int id);
        Task<Result<List<Products>>> GetFilteredProductsAsync(ProductFilterDto filter);
        Task<Result<List<string>>> GetCategoriesAsync();
        Task<Result<List<string>>> GetBrandsAsync();
    }
}
