using GroceryStoreApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStoreApp.Controllers;

[ApiController]
[Route("api/sales")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _saleService;

    public SalesController(ISaleService saleService)
    {
        _saleService = saleService;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveSales()
    {
        var sales = await _saleService.GetActiveSalesAsync();
        return Ok(sales);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSale(int id)
    {
        var sale = await _saleService.GetSaleByIdAsync(id);
        return sale == null ? NotFound() : Ok(sale);
    }
}
