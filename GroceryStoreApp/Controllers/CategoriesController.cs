using GroceryStoreApp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStoreApp.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IProductService _productService;

    public CategoriesController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{slug}/products")]
    public async Task<IActionResult> GetProductsByCategory(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _productService.GetProductsByCategorySlugAsync(slug, page, pageSize);
        return Ok(result);
    }
}
