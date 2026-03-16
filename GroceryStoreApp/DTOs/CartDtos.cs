using System.ComponentModel.DataAnnotations;

namespace GroceryStoreApp.DTOs;

public class CartDto
{
    public int Id { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public decimal Total { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public ActiveSaleDto? ActiveSale { get; set; }
}

public class AddToCartRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required, Range(1, 100)]
    public int Quantity { get; set; }
}

public class UpdateCartItemRequest
{
    [Required, Range(1, 100)]
    public int Quantity { get; set; }
}
