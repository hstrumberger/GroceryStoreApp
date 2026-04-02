using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Implementations;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GroceryStoreApp.Tests.Services;

public class CartServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly Mock<ISaleService> _saleService;
    private readonly CartService _cartService;

    private const string UserId = "test-user-1";

    public CartServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        _saleService = new Mock<ISaleService>();
        _saleService.Setup(s => s.GetApplicableSaleForProductAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((Sale?)null);

        _cartService = new CartService(_db, _saleService.Object);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Product> AddProductAsync(int id = 1, decimal price = 5.00m, int stock = 100)
    {
        var product = new Product
        {
            Id = id,
            Sku = $"SKU-{id:000}",
            Name = $"Product {id}",
            CategoryId = 1,
            Price = price,
            StockQuantity = stock,
            IsActive = true,
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    private async Task<Cart> AddCartAsync()
    {
        var cart = new Cart { UserId = UserId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    // ── GetCartAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCartAsync_NoCart_CreatesAndReturnsEmptyCart()
    {
        var result = await _cartService.GetCartAsync(UserId);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0m, result.Total);
    }

    [Fact]
    public async Task GetCartAsync_ExistingCart_ReturnsCart()
    {
        await AddCartAsync();

        var result = await _cartService.GetCartAsync(UserId);

        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    // ── AddItemAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddItemAsync_NewProduct_AddsCartItem()
    {
        await AddProductAsync(1);

        var result = await _cartService.AddItemAsync(UserId, new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 2
        });

        Assert.Single(result.Items);
        Assert.Equal(1, result.Items[0].ProductId);
        Assert.Equal(2, result.Items[0].Quantity);
    }

    [Fact]
    public async Task AddItemAsync_ExistingCartItem_IncreasesQuantity()
    {
        await AddProductAsync(1, stock: 50);
        var cart = await AddCartAsync();
        _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = 1, Quantity = 3 });
        await _db.SaveChangesAsync();

        var result = await _cartService.AddItemAsync(UserId, new AddToCartRequest
        {
            ProductId = 1,
            Quantity = 2
        });

        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].Quantity);
    }

    [Fact]
    public async Task AddItemAsync_ProductNotFound_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cartService.AddItemAsync(UserId, new AddToCartRequest
            {
                ProductId = 999,
                Quantity = 1
            }));
    }

    [Fact]
    public async Task AddItemAsync_InsufficientStock_ThrowsInvalidOperationException()
    {
        await AddProductAsync(1, stock: 2);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cartService.AddItemAsync(UserId, new AddToCartRequest
            {
                ProductId = 1,
                Quantity = 10
            }));
    }

    [Fact]
    public async Task AddItemAsync_InactiveProduct_ThrowsInvalidOperationException()
    {
        var product = new Product
        {
            Id = 5,
            Sku = "SKU-005",
            Name = "Inactive Product",
            CategoryId = 1,
            Price = 1.00m,
            StockQuantity = 100,
            IsActive = false,
        };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _cartService.AddItemAsync(UserId, new AddToCartRequest
            {
                ProductId = 5,
                Quantity = 1
            }));
    }

    // ── RemoveItemAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveItemAsync_ExistingItem_RemovesItem()
    {
        await AddProductAsync(1);
        var cart = await AddCartAsync();
        _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = 1, Quantity = 1 });
        await _db.SaveChangesAsync();

        var result = await _cartService.RemoveItemAsync(UserId, 1);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task RemoveItemAsync_NonExistentItem_ReturnsCartUnchanged()
    {
        await AddProductAsync(1);
        var cart = await AddCartAsync();
        _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = 1, Quantity = 1 });
        await _db.SaveChangesAsync();

        var result = await _cartService.RemoveItemAsync(UserId, 999);

        Assert.Single(result.Items);
    }

    // ── UpdateItemAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateItemAsync_ExistingItem_UpdatesQuantity()
    {
        await AddProductAsync(1, stock: 50);
        var cart = await AddCartAsync();
        _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = 1, Quantity = 1 });
        await _db.SaveChangesAsync();

        var result = await _cartService.UpdateItemAsync(UserId, 1, new UpdateCartItemRequest { Quantity = 5 });

        Assert.Equal(5, result.Items[0].Quantity);
    }

    [Fact]
    public async Task UpdateItemAsync_ItemNotFound_ThrowsKeyNotFoundException()
    {
        await AddCartAsync();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _cartService.UpdateItemAsync(UserId, 999, new UpdateCartItemRequest { Quantity = 1 }));
    }

    // ── ClearCartAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task ClearCartAsync_CartWithItems_RemovesAllItems()
    {
        await AddProductAsync(1);
        await AddProductAsync(2);
        var cart = await AddCartAsync();
        _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = 1, Quantity = 1 });
        _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = 2, Quantity = 2 });
        await _db.SaveChangesAsync();

        await _cartService.ClearCartAsync(UserId);

        var items = await _db.CartItems.Where(ci => ci.CartId == cart.Id).ToListAsync();
        Assert.Empty(items);
    }

    [Fact]
    public async Task ClearCartAsync_NoCart_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(() =>
            _cartService.ClearCartAsync("nonexistent-user"));
        Assert.Null(exception);
    }
}
