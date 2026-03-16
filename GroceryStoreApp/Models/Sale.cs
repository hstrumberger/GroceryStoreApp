namespace GroceryStoreApp.Models;

public class Sale
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DiscountType { get; set; } = "Percentage"; // "Percentage" or "FixedAmount"
    public decimal DiscountValue { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SaleProduct> SaleProducts { get; set; } = [];
    public ICollection<SaleCategory> SaleCategories { get; set; } = [];
}
