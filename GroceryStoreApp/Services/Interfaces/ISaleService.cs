using GroceryStoreApp.DTOs;
using GroceryStoreApp.Models;

namespace GroceryStoreApp.Services.Interfaces;

public interface ISaleService
{
    Task<List<SaleDto>> GetActiveSalesAsync();
    Task<SaleDto?> GetSaleByIdAsync(int id);
    Task<Sale?> GetApplicableSaleForProductAsync(int productId, int categoryId);
    decimal CalculateDiscountedPrice(decimal price, Sale sale);
}
