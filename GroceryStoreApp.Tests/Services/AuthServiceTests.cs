using GroceryStoreApp.Data;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;
using GroceryStoreApp.Services.Implementations;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GroceryStoreApp.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<ITokenService> _tokenService;
    private readonly AppDbContext _db;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManager = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManager = new Mock<SignInManager<User>>(
            _userManager.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _tokenService = new Mock<ITokenService>();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AppDbContext(options);

        _authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            _tokenService.Object,
            _db);
    }

    public void Dispose() => _db.Dispose();

    private void SetupTokenService()
    {
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access-token");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
        _tokenService.Setup(t => t.StoreRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
    }

    // ── RegisterAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        _userManager.Setup(m => m.FindByEmailAsync("new@example.com"))
            .ReturnsAsync((User?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "Password1!"))
            .ReturnsAsync(IdentityResult.Success);
        SetupTokenService();

        var result = await _authService.RegisterAsync(new RegisterRequest
        {
            Email = "new@example.com",
            Password = "Password1!",
            FirstName = "Jane",
            LastName = "Doe"
        });

        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("new@example.com", result.User.Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        _userManager.Setup(m => m.FindByEmailAsync("existing@example.com"))
            .ReturnsAsync(new User { Email = "existing@example.com" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.RegisterAsync(new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password1!",
                FirstName = "Jane",
                LastName = "Doe"
            }));
    }

    [Fact]
    public async Task RegisterAsync_CreateFails_ThrowsInvalidOperationException()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak." }));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.RegisterAsync(new RegisterRequest
            {
                Email = "new@example.com",
                Password = "weak",
                FirstName = "Jane",
                LastName = "Doe"
            }));
    }

    // ── LoginAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var user = new User { Email = "user@example.com", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _signInManager.Setup(m => m.CheckPasswordSignInAsync(user, "Password1!", true))
            .ReturnsAsync(SignInResult.Success);
        SetupTokenService();

        var result = await _authService.LoginAsync(new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password1!"
        });

        Assert.NotNull(result);
        Assert.Equal("access-token", result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(new LoginRequest
            {
                Email = "nobody@example.com",
                Password = "Password1!"
            }));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Email = "inactive@example.com", IsActive = false };
        _userManager.Setup(m => m.FindByEmailAsync("inactive@example.com")).ReturnsAsync(user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(new LoginRequest
            {
                Email = "inactive@example.com",
                Password = "Password1!"
            }));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Email = "user@example.com", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
        _signInManager.Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
            .ReturnsAsync(SignInResult.Failed);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(new LoginRequest
            {
                Email = "user@example.com",
                Password = "wrongpassword"
            }));
    }

    [Fact]
    public async Task LoginAsync_LockedOut_ThrowsUnauthorizedAccessExceptionWithLockoutMessage()
    {
        var user = new User { Email = "locked@example.com", IsActive = true };
        _userManager.Setup(m => m.FindByEmailAsync("locked@example.com")).ReturnsAsync(user);
        _signInManager.Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), true))
            .ReturnsAsync(SignInResult.LockedOut);

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(new LoginRequest
            {
                Email = "locked@example.com",
                Password = "Password1!"
            }));

        Assert.Contains("locked", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ── GetMeAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMeAsync_ExistingUser_ReturnsUserDto()
    {
        var user = new User { Id = "user-123", Email = "me@example.com", FirstName = "John", LastName = "Smith" };
        _userManager.Setup(m => m.FindByIdAsync("user-123")).ReturnsAsync(user);

        var result = await _authService.GetMeAsync("user-123");

        Assert.Equal("user-123", result.Id);
        Assert.Equal("me@example.com", result.Email);
        Assert.Equal("John", result.FirstName);
    }

    [Fact]
    public async Task GetMeAsync_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _authService.GetMeAsync("nonexistent"));
    }
}
