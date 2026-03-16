using GroceryStoreApp.DTOs;

namespace GroceryStoreApp.Services.Interfaces;

public interface ISearchService
{
    Task<PagedResult<ProductDto>> SearchAsync(string? query, int? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize);
}
