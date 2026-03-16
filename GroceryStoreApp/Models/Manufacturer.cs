namespace GroceryStoreApp.Models;

public class Manufacturer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Country { get; set; }
    public string? ContactEmail { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<Product> Products { get; set; } = [];
}
