using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Services.Implementations;

public class CartService : ICartService
{
    private readonly AppDbContext _db;
    private readonly ISaleService _saleService;

    public CartService(AppDbContext db, ISaleService saleService)
    {
        _db = db;
        _saleService = saleService;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartAsync(userId);
        return await MapToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddToCartRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var product = await _db.Products.FindAsync(request.ProductId)
            ?? throw new KeyNotFoundException("Product not found.");

        if (!product.IsActive || product.StockQuantity < request.Quantity)
            throw new InvalidOperationException("Product unavailable or insufficient stock.");

        var existing = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == request.ProductId);

        if (existing != null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateItemAsync(string userId, int productId, UpdateCartItemRequest request)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var item = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId)
            ?? throw new KeyNotFoundException("Cart item not found.");

        var product = await _db.Products.FindAsync(productId)!;
        if (product != null && product.StockQuantity < request.Quantity)
            throw new InvalidOperationException("Insufficient stock.");

        item.Quantity = request.Quantity;
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveItemAsync(string userId, int productId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        var item = await _db.CartItems
            .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

        if (item != null)
        {
            _db.CartItems.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return await GetCartAsync(userId);
    }

    public async Task ClearCartAsync(string userId)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return;

        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task<Cart> GetOrCreateCartAsync(string userId)
    {
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null) return cart;

        cart = new Cart { UserId = userId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    private async Task<CartDto> MapToDto(Cart cart)
    {
        var items = await _db.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .Where(ci => ci.CartId == cart.Id)
            .ToListAsync();

        var itemDtos = new List<CartItemDto>();
        decimal total = 0;

        foreach (var item in items)
        {
            var sale = await _saleService.GetApplicableSaleForProductAsync(item.ProductId, item.Product.CategoryId);
            var discounted = sale != null ? _saleService.CalculateDiscountedPrice(item.Product.Price, sale) : (decimal?)null;
            var effectivePrice = discounted ?? item.Product.Price;
            var lineTotal = effectivePrice * item.Quantity;
            total += lineTotal;

            itemDtos.Add(new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                ProductSku = item.Product.Sku,
                UnitPrice = item.Product.Price,
                DiscountedPrice = discounted,
                Quantity = item.Quantity,
                LineTotal = lineTotal,
                StockQuantity = item.Product.StockQuantity,
                ImageUrl = item.Product.Images.FirstOrDefault(i => i.IsPrimary)?.Url
                        ?? item.Product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.Url,
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

        return new CartDto
        {
            Id = cart.Id,
            Items = itemDtos,
            Total = total
        };
    }
}
