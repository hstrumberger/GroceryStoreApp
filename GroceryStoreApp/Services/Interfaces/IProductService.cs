using GroceryStoreApp.DTOs;

namespace GroceryStoreApp.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(int? categoryId, int page, int pageSize, string? sortBy);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<PagedResult<ProductDto>> GetProductsByCategorySlugAsync(string slug, int page, int pageSize);
}
