using System.Security.Claims;
using GroceryStoreApp.DTOs;
using GroceryStoreApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStoreApp.Controllers;

[Authorize]
[ApiController]
[Route("api/checkout")]
public class CheckoutController : ControllerBase
{
    private readonly IOrderService _orderService;

    public CheckoutController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        var order = await _orderService.CheckoutAsync(UserId, request);
        return Ok(order);
    }
}
