using GroceryStoreApp.Models;

namespace GroceryStoreApp.Services.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
    Task StoreRefreshTokenAsync(string userId, string rawToken);
    Task<User?> ValidateRefreshTokenAsync(string rawToken);
    Task RevokeRefreshTokenAsync(string rawToken, string? replacedByToken = null);
}
