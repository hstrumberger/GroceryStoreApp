using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GroceryStoreApp.Services.Implementations;

public class SaleService : ISaleService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private const string ActiveSalesCacheKey = "active_sales";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public SaleService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<List<SaleDto>> GetActiveSalesAsync()
    {
        if (_cache.TryGetValue(ActiveSalesCacheKey, out List<SaleDto>? cached) && cached != null)
            return cached;

        var now = DateTime.UtcNow;
        var sales = await _db.Sales
            .Include(s => s.SaleProducts)
            .Include(s => s.SaleCategories)
            .Where(s => s.IsActive && s.StartDate <= now && s.EndDate >= now)
            .ToListAsync();

        var dtos = sales.Select(MapToDto).ToList();
        _cache.Set(ActiveSalesCacheKey, dtos, CacheTtl);
        return dtos;
    }

    public async Task<SaleDto?> GetSaleByIdAsync(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.SaleProducts)
            .Include(s => s.SaleCategories)
            .FirstOrDefaultAsync(s => s.Id == id);

        return sale == null ? null : MapToDto(sale);
    }

    public async Task<Sale?> GetApplicableSaleForProductAsync(int productId, int categoryId)
    {
        var now = DateTime.UtcNow;
        var activeSales = await _db.Sales
            .Include(s => s.SaleProducts)
            .Include(s => s.SaleCategories)
            .Where(s => s.IsActive && s.StartDate <= now && s.EndDate >= now)
            .ToListAsync();

        Sale? bestSale = null;
        decimal bestDiscount = 0;

        foreach (var sale in activeSales)
        {
            bool applies = sale.SaleProducts.Any(sp => sp.ProductId == productId)
                        || sale.SaleCategories.Any(sc => sc.CategoryId == categoryId);

            if (!applies) continue;

            if (sale.DiscountValue > bestDiscount)
            {
                bestDiscount = sale.DiscountValue;
                bestSale = sale;
            }
        }

        return bestSale;
    }

    public decimal CalculateDiscountedPrice(decimal price, Sale sale)
    {
        return sale.DiscountType == "Percentage"
            ? Math.Round(price * (1 - sale.DiscountValue / 100), 2)
            : Math.Max(0, Math.Round(price - sale.DiscountValue, 2));
    }

    private static SaleDto MapToDto(Sale sale) => new()
    {
        Id = sale.Id,
        Name = sale.Name,
        Description = sale.Description,
        StartDate = sale.StartDate,
        EndDate = sale.EndDate,
        DiscountType = sale.DiscountType,
        DiscountValue = sale.DiscountValue,
        IsActive = sale.IsActive,
        ProductIds = sale.SaleProducts.Select(sp => sp.ProductId).ToList(),
        CategoryIds = sale.SaleCategories.Select(sc => sc.CategoryId).ToList()
    };
}
