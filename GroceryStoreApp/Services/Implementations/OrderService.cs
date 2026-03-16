using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ISaleService _saleService;
    private const decimal TaxRate = 0.08m;
    private const decimal ShippingRate = 9.99m;
    private const decimal FreeShippingThreshold = 75m;

    public OrderService(AppDbContext db, ISaleService saleService)
    {
        _db = db;
        _saleService = saleService;
    }

    public async Task<OrderDto> CheckoutAsync(string userId, CheckoutRequest request)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw new InvalidOperationException("Cart not found.");

            if (!cart.Items.Any())
                throw new InvalidOperationException("Cart is empty.");

            // Verify stock and compute pricing
            var orderItems = new List<OrderItem>();
            decimal subTotal = 0;
            decimal discountTotal = 0;

            foreach (var cartItem in cart.Items)
            {
                var product = cartItem.Product;
                if (!product.IsActive)
                    throw new InvalidOperationException($"Product '{product.Name}' is no longer available.");
                if (product.StockQuantity < cartItem.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}.");

                var sale = await _saleService.GetApplicableSaleForProductAsync(product.Id, product.CategoryId);
                var discountedPrice = sale != null ? _saleService.CalculateDiscountedPrice(product.Price, sale) : product.Price;
                var lineTotal = discountedPrice * cartItem.Quantity;

                subTotal += product.Price * cartItem.Quantity;
                discountTotal += (product.Price - discountedPrice) * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
                    UnitPrice = product.Price,
                    DiscountedPrice = discountedPrice,
                    LineTotal = lineTotal,
                    Quantity = cartItem.Quantity
                });

                // Decrement stock
                product.StockQuantity -= cartItem.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }

            var taxableAmount = subTotal - discountTotal;
            var tax = Math.Round(taxableAmount * TaxRate, 2);
            var shipping = taxableAmount >= FreeShippingThreshold ? 0m : ShippingRate;
            var total = taxableAmount + tax + shipping;

            // Simulate payment (always succeeds)
            var last4 = request.Payment.CardNumber.Length >= 4
                ? request.Payment.CardNumber[^4..]
                : null;

            var order = new Order
            {
                UserId = userId,
                Status = "Processing",
                SubTotal = subTotal,
                DiscountAmount = discountTotal,
                TaxAmount = tax,
                ShippingAmount = shipping,
                Total = total,
                ShippingFirstName = request.ShippingAddress.FirstName,
                ShippingLastName = request.ShippingAddress.LastName,
                ShippingAddressLine1 = request.ShippingAddress.AddressLine1,
                ShippingAddressLine2 = request.ShippingAddress.AddressLine2,
                ShippingCity = request.ShippingAddress.City,
                ShippingState = request.ShippingAddress.State,
                ShippingPostalCode = request.ShippingAddress.PostalCode,
                ShippingCountry = request.ShippingAddress.Country,
                PaymentMethod = request.Payment.Method,
                PaymentLast4 = last4,
                Items = orderItems
            };

            _db.Orders.Add(order);

            // Clear cart
            _db.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToDto(order);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<OrderSummaryDto>> GetOrdersAsync(string userId)
    {
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                Status = o.Status,
                Total = o.Total,
                ItemCount = o.Items.Count,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order == null ? null : MapToDto(order);
    }

    private static OrderDto MapToDto(Order order) => new()
    {
        Id = order.Id,
        Status = order.Status,
        SubTotal = order.SubTotal,
        DiscountAmount = order.DiscountAmount,
        TaxAmount = order.TaxAmount,
        ShippingAmount = order.ShippingAmount,
        Total = order.Total,
        ShippingAddress = new ShippingAddressDto
        {
            FirstName = order.ShippingFirstName,
            LastName = order.ShippingLastName,
            AddressLine1 = order.ShippingAddressLine1,
            AddressLine2 = order.ShippingAddressLine2,
            City = order.ShippingCity,
            State = order.ShippingState,
            PostalCode = order.ShippingPostalCode,
            Country = order.ShippingCountry
        },
        PaymentMethod = order.PaymentMethod,
        PaymentLast4 = order.PaymentLast4,
        CreatedAt = order.CreatedAt,
        Items = order.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            ProductSku = i.ProductSku,
            UnitPrice = i.UnitPrice,
            DiscountedPrice = i.DiscountedPrice,
            LineTotal = i.LineTotal,
            Quantity = i.Quantity
        }).ToList()
    };
}
