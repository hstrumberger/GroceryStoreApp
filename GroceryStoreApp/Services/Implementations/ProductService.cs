using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Services.Implementations;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly ISaleService _saleService;

    public ProductService(AppDbContext db, ISaleService saleService)
    {
        _db = db;
        _saleService = saleService;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(int? categoryId, int page, int pageSize, string? sortBy)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Images)
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var products = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var dtos = new List<ProductDto>();
        foreach (var p in products)
        {
            var sale = await _saleService.GetApplicableSaleForProductAsync(p.Id, p.CategoryId);
            dtos.Add(MapToDto(p, sale != null ? _saleService.CalculateDiscountedPrice(p.Price, sale) : null, sale));
        }

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _db.Products
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (product == null) return null;

        var sale = await _saleService.GetApplicableSaleForProductAsync(product.Id, product.CategoryId);
        return MapToDto(product, sale != null ? _saleService.CalculateDiscountedPrice(product.Price, sale) : null, sale);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _db.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return categories.Select(MapCategoryToDto).ToList();
    }

    public async Task<PagedResult<ProductDto>> GetProductsByCategorySlugAsync(string slug, int page, int pageSize)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        if (category == null)
            return new PagedResult<ProductDto> { Page = page, PageSize = pageSize };

        return await GetProductsAsync(category.Id, page, pageSize, null);
    }

    private static ProductDto MapToDto(GroceryStoreApp.Models.Product p, decimal? discountedPrice, GroceryStoreApp.Models.Sale? sale) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        DiscountedPrice = discountedPrice,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        ManufacturerId = p.ManufacturerId,
        ManufacturerName = p.Manufacturer?.Name,
        AverageRating = p.AverageRating,
        RatingCount = p.RatingCount,
        StockQuantity = p.StockQuantity,
        IsActive = p.IsActive,
        Images = p.Images.OrderBy(i => i.DisplayOrder).Select(i => new ProductImageDto
        {
            Id = i.Id,
            Url = i.Url,
            AltText = i.AltText,
            DisplayOrder = i.DisplayOrder,
            IsPrimary = i.IsPrimary
        }).ToList(),
        ActiveSale = sale == null ? null : new ActiveSaleDto
        {
            Id = sale.Id,
            Name = sale.Name,
            DiscountType = sale.DiscountType,
            DiscountValue = sale.DiscountValue,
            EndDate = sale.EndDate
        }
    };

    private static CategoryDto MapCategoryToDto(GroceryStoreApp.Models.Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ParentCategoryId = c.ParentCategoryId,
        DisplayOrder = c.DisplayOrder,
        SubCategories = c.SubCategories.OrderBy(sc => sc.DisplayOrder).Select(MapCategoryToDto).ToList()
    };
}
