using GroceryStoreApp.DTOs;

namespace GroceryStoreApp.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CheckoutAsync(string userId, CheckoutRequest request);
    Task<List<OrderSummaryDto>> GetOrdersAsync(string userId);
    Task<OrderDto?> GetOrderByIdAsync(string userId, int orderId);
}
