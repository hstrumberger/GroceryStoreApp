using GroceryStoreApp.DTOs;

namespace GroceryStoreApp.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddItemAsync(string userId, AddToCartRequest request);
    Task<CartDto> UpdateItemAsync(string userId, int productId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemAsync(string userId, int productId);
    Task ClearCartAsync(string userId);
}
