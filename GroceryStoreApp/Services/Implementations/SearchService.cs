using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Services.Implementations;

public class SearchService : ISearchService
{
    private readonly AppDbContext _db;
    private readonly ISaleService _saleService;

    public SearchService(AppDbContext db, ISaleService saleService)
    {
        _db = db;
        _saleService = saleService;
    }

    public async Task<PagedResult<ProductDto>> SearchAsync(string? query, int? categoryId, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
    {
        var q = _db.Products
            .Include(p => p.Category)
            .Include(p => p.Manufacturer)
            .Include(p => p.Images)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lower = query.ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(lower)
                           || (p.Description != null && p.Description.ToLower().Contains(lower))
                           || p.Sku.ToLower().Contains(lower));
        }

        if (categoryId.HasValue)
            q = q.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            q = q.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            q = q.Where(p => p.Price <= maxPrice.Value);

        q = q.OrderBy(p => p.Name);

        var total = await q.CountAsync();
        var products = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var dtos = new List<ProductDto>();
        foreach (var p in products)
        {
            var sale = await _saleService.GetApplicableSaleForProductAsync(p.Id, p.CategoryId);
            var discountedPrice = sale != null ? _saleService.CalculateDiscountedPrice(p.Price, sale) : (decimal?)null;
            dtos.Add(new ProductDto
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
            });
        }

        return new PagedResult<ProductDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }
}
