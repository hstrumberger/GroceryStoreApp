using GroceryStoreApp.Data;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace GroceryStoreApp.Tests.Services;

public class SaleServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly SaleService _saleService;

    public SaleServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);
        _saleService = new SaleService(_db, new MemoryCache(new MemoryCacheOptions()));
    }

    public void Dispose() => _db.Dispose();

    // ── CalculateDiscountedPrice ─────────────────────────────────────────────

    [Fact]
    public void CalculateDiscountedPrice_Percentage_ReturnsDiscountedPrice()
    {
        var sale = new Sale { DiscountType = "Percentage", DiscountValue = 20 };
        var result = _saleService.CalculateDiscountedPrice(10.00m, sale);
        Assert.Equal(8.00m, result);
    }

    [Fact]
    public void CalculateDiscountedPrice_Percentage_RoundsToTwoDecimalPlaces()
    {
        var sale = new Sale { DiscountType = "Percentage", DiscountValue = 33 };
        var result = _saleService.CalculateDiscountedPrice(9.99m, sale);
        Assert.Equal(Math.Round(9.99m * 0.67m, 2), result);
    }

    [Fact]
    public void CalculateDiscountedPrice_FixedAmount_ReturnsDiscountedPrice()
    {
        var sale = new Sale { DiscountType = "FixedAmount", DiscountValue = 3.00m };
        var result = _saleService.CalculateDiscountedPrice(10.00m, sale);
        Assert.Equal(7.00m, result);
    }

    [Fact]
    public void CalculateDiscountedPrice_FixedAmount_NeverGoesNegative()
    {
        var sale = new Sale { DiscountType = "FixedAmount", DiscountValue = 100.00m };
        var result = _saleService.CalculateDiscountedPrice(5.00m, sale);
        Assert.Equal(0.00m, result);
    }

    [Fact]
    public void CalculateDiscountedPrice_FullPercentageDiscount_ReturnsZero()
    {
        var sale = new Sale { DiscountType = "Percentage", DiscountValue = 100 };
        var result = _saleService.CalculateDiscountedPrice(25.00m, sale);
        Assert.Equal(0.00m, result);
    }

    // ── GetApplicableSaleForProductAsync ─────────────────────────────────────

    [Fact]
    public async Task GetApplicableSaleForProductAsync_NoSales_ReturnsNull()
    {
        var result = await _saleService.GetApplicableSaleForProductAsync(1, 1);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetApplicableSaleForProductAsync_ActiveProductSale_ReturnsSale()
    {
        var sale = new Sale
        {
            Id = 1,
            Name = "Summer Sale",
            DiscountType = "Percentage",
            DiscountValue = 20,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        _db.Sales.Add(sale);
        _db.SaleProducts.Add(new SaleProduct { SaleId = 1, ProductId = 42 });
        await _db.SaveChangesAsync();

        var result = await _saleService.GetApplicableSaleForProductAsync(42, 99);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetApplicableSaleForProductAsync_ActiveCategorySale_ReturnsSale()
    {
        var sale = new Sale
        {
            Id = 2,
            Name = "Category Sale",
            DiscountType = "FixedAmount",
            DiscountValue = 5,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        _db.Sales.Add(sale);
        _db.SaleCategories.Add(new SaleCategory { SaleId = 2, CategoryId = 10 });
        await _db.SaveChangesAsync();

        var result = await _saleService.GetApplicableSaleForProductAsync(999, 10);

        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task GetApplicableSaleForProductAsync_ExpiredSale_ReturnsNull()
    {
        var sale = new Sale
        {
            Id = 3,
            Name = "Expired Sale",
            DiscountType = "Percentage",
            DiscountValue = 10,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
        };
        _db.Sales.Add(sale);
        _db.SaleProducts.Add(new SaleProduct { SaleId = 3, ProductId = 1 });
        await _db.SaveChangesAsync();

        var result = await _saleService.GetApplicableSaleForProductAsync(1, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetApplicableSaleForProductAsync_MultipleSales_ReturnsBestDiscount()
    {
        var lowSale = new Sale
        {
            Id = 4,
            Name = "Low Sale",
            DiscountType = "Percentage",
            DiscountValue = 10,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        var highSale = new Sale
        {
            Id = 5,
            Name = "High Sale",
            DiscountType = "Percentage",
            DiscountValue = 30,
            IsActive = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        _db.Sales.AddRange(lowSale, highSale);
        _db.SaleProducts.Add(new SaleProduct { SaleId = 4, ProductId = 7 });
        _db.SaleProducts.Add(new SaleProduct { SaleId = 5, ProductId = 7 });
        await _db.SaveChangesAsync();

        var result = await _saleService.GetApplicableSaleForProductAsync(7, 99);

        Assert.NotNull(result);
        Assert.Equal(5, result.Id); // highest discount wins
    }

    [Fact]
    public async Task GetApplicableSaleForProductAsync_InactiveSale_ReturnsNull()
    {
        var sale = new Sale
        {
            Id = 6,
            Name = "Inactive Sale",
            DiscountType = "Percentage",
            DiscountValue = 50,
            IsActive = false,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
        };
        _db.Sales.Add(sale);
        _db.SaleProducts.Add(new SaleProduct { SaleId = 6, ProductId = 1 });
        await _db.SaveChangesAsync();

        var result = await _saleService.GetApplicableSaleForProductAsync(1, 1);

        Assert.Null(result);
    }
}
