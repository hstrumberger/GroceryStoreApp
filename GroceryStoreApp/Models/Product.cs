namespace GroceryStoreApp.Models;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int? ManufacturerId { get; set; }
    public Manufacturer? Manufacturer { get; set; }
    public decimal WeightGrams { get; set; }
    public decimal DimensionLengthCm { get; set; }
    public decimal DimensionWidthCm { get; set; }
    public decimal DimensionHeightCm { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<SaleProduct> SaleProducts { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
