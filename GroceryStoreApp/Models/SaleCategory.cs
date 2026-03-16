namespace GroceryStoreApp.Models;

public class SaleCategory
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}
