using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly AppDbContext _db;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager,
        ITokenService tokenService, AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _db = db;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        // Create a cart for the new user
        _db.Carts.Add(new Cart { UserId = user.Id });
        await _db.SaveChangesAsync();

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                throw new UnauthorizedAccessException("Account is locked out. Try again later.");
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var user = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
        if (user == null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, newRefreshToken);
        await _tokenService.StoreRefreshTokenAsync(user.Id, newRefreshToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        return new AuthResponse
        {
            AccessToken = accessToken,
            User = MapToDto(user),
            RefreshToken = newRefreshToken
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<UserDto> GetMeAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        return MapToDto(user);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        await _tokenService.StoreRefreshTokenAsync(user.Id, refreshToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            User = MapToDto(user),
            RefreshToken = refreshToken
        };
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        FirstName = user.FirstName,
        LastName = user.LastName
    };
}
