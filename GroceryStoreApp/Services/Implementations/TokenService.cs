using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GroceryStoreApp.Data;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GroceryStoreApp.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public TokenService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }

    public async Task StoreRefreshTokenAsync(string userId, string rawToken)
    {
        var expirationDays = int.Parse(_config["JwtSettings:RefreshTokenExpirationDays"]!);
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();
    }

    public async Task<User?> ValidateRefreshTokenAsync(string rawToken)
    {
        var hashed = HashToken(rawToken);
        var token = await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == hashed);

        if (token == null || !token.IsActive) return null;
        return token.User;
    }

    public async Task RevokeRefreshTokenAsync(string rawToken, string? replacedByToken = null)
    {
        var hashed = HashToken(rawToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hashed);
        if (token == null) return;

        token.RevokedAt = DateTime.UtcNow;
        if (replacedByToken != null)
            token.ReplacedByToken = HashToken(replacedByToken);

        await _db.SaveChangesAsync();
    }
}
