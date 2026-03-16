namespace GroceryStoreApp.DTOs;

public class SaleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public bool IsActive { get; set; }
    public List<int> ProductIds { get; set; } = [];
    public List<int> CategoryIds { get; set; } = [];
}
