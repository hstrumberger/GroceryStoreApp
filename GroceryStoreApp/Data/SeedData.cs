using GroceryStoreApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Categories.AnyAsync()) return; // Already seeded

        // Categories
        var produce = new Category { Name = "Produce", Slug = "produce", DisplayOrder = 1, Description = "Fresh fruits and vegetables" };
        var dairy = new Category { Name = "Dairy & Eggs", Slug = "dairy-eggs", DisplayOrder = 2, Description = "Milk, cheese, yogurt, eggs" };
        var bakery = new Category { Name = "Bakery", Slug = "bakery", DisplayOrder = 3, Description = "Breads, pastries, cakes" };
        var meat = new Category { Name = "Meat & Seafood", Slug = "meat-seafood", DisplayOrder = 4, Description = "Fresh and frozen meats" };
        var pantry = new Category { Name = "Pantry", Slug = "pantry", DisplayOrder = 5, Description = "Canned goods, pasta, rice" };
        var beverages = new Category { Name = "Beverages", Slug = "beverages", DisplayOrder = 6, Description = "Juices, sodas, water" };
        var frozen = new Category { Name = "Frozen Foods", Slug = "frozen", DisplayOrder = 7, Description = "Frozen meals and snacks" };

        db.Categories.AddRange(produce, dairy, bakery, meat, pantry, beverages, frozen);
        await db.SaveChangesAsync();

        // Manufacturers
        var organic = new Manufacturer { Name = "Green Valley Organics", Country = "USA", Website = "https://example.com" };
        var freshFarm = new Manufacturer { Name = "Fresh Farm Co.", Country = "USA" };
        var artisanBakery = new Manufacturer { Name = "Artisan Bakers Guild", Country = "USA" };

        db.Manufacturers.AddRange(organic, freshFarm, artisanBakery);
        await db.SaveChangesAsync();

        // Products
        var products = new List<Product>
        {
            // Produce
            new() { Sku = "PROD-001", Name = "Organic Bananas", Price = 1.29m, CategoryId = produce.Id, ManufacturerId = organic.Id, StockQuantity = 150, WeightGrams = 500, Description = "A bunch of ripe organic bananas." },
            new() { Sku = "PROD-002", Name = "Fresh Strawberries", Price = 4.99m, CategoryId = produce.Id, StockQuantity = 80, WeightGrams = 340, Description = "Sweet, fresh-picked strawberries." },
            new() { Sku = "PROD-003", Name = "Baby Spinach", Price = 3.49m, CategoryId = produce.Id, ManufacturerId = organic.Id, StockQuantity = 60, WeightGrams = 170, Description = "Tender organic baby spinach." },
            new() { Sku = "PROD-004", Name = "Avocados (4-pack)", Price = 5.99m, CategoryId = produce.Id, StockQuantity = 45, WeightGrams = 600, Description = "Ripe Hass avocados ready to eat." },
            new() { Sku = "PROD-005", Name = "Roma Tomatoes (lb)", Price = 1.79m, CategoryId = produce.Id, ManufacturerId = freshFarm.Id, StockQuantity = 100, WeightGrams = 453, Description = "Firm roma tomatoes, perfect for sauces." },

            // Dairy
            new() { Sku = "DAIR-001", Name = "Whole Milk (1 gal)", Price = 4.29m, CategoryId = dairy.Id, ManufacturerId = freshFarm.Id, StockQuantity = 90, WeightGrams = 3785, Description = "Fresh whole milk from local farms." },
            new() { Sku = "DAIR-002", Name = "Free-Range Eggs (dozen)", Price = 5.49m, CategoryId = dairy.Id, ManufacturerId = organic.Id, StockQuantity = 120, WeightGrams = 680, Description = "Free-range organic large eggs." },
            new() { Sku = "DAIR-003", Name = "Sharp Cheddar Cheese", Price = 6.99m, CategoryId = dairy.Id, StockQuantity = 55, WeightGrams = 340, Description = "Aged sharp cheddar, sliced." },
            new() { Sku = "DAIR-004", Name = "Greek Yogurt (32oz)", Price = 7.49m, CategoryId = dairy.Id, ManufacturerId = organic.Id, StockQuantity = 40, WeightGrams = 907, Description = "Creamy plain Greek yogurt, high protein." },

            // Bakery
            new() { Sku = "BAKE-001", Name = "Sourdough Bread Loaf", Price = 6.49m, CategoryId = bakery.Id, ManufacturerId = artisanBakery.Id, StockQuantity = 30, WeightGrams = 680, Description = "Classic sourdough baked fresh daily." },
            new() { Sku = "BAKE-002", Name = "Blueberry Muffins (6pk)", Price = 5.99m, CategoryId = bakery.Id, ManufacturerId = artisanBakery.Id, StockQuantity = 25, WeightGrams = 420, Description = "Freshly baked blueberry muffins." },
            new() { Sku = "BAKE-003", Name = "Whole Wheat Bagels (6pk)", Price = 4.49m, CategoryId = bakery.Id, StockQuantity = 35, WeightGrams = 480, Description = "Hearty whole wheat bagels." },

            // Meat
            new() { Sku = "MEAT-001", Name = "Chicken Breast (2lb)", Price = 9.99m, CategoryId = meat.Id, ManufacturerId = freshFarm.Id, StockQuantity = 50, WeightGrams = 907, Description = "Boneless skinless chicken breasts." },
            new() { Sku = "MEAT-002", Name = "Ground Beef 80/20 (1lb)", Price = 6.49m, CategoryId = meat.Id, StockQuantity = 60, WeightGrams = 453, Description = "Fresh 80/20 ground beef." },
            new() { Sku = "MEAT-003", Name = "Atlantic Salmon Fillet", Price = 12.99m, CategoryId = meat.Id, StockQuantity = 25, WeightGrams = 340, Description = "Fresh Atlantic salmon fillet." },

            // Pantry
            new() { Sku = "PANT-001", Name = "Organic Pasta (16oz)", Price = 2.49m, CategoryId = pantry.Id, ManufacturerId = organic.Id, StockQuantity = 200, WeightGrams = 453, Description = "Organic semolina penne pasta." },
            new() { Sku = "PANT-002", Name = "Jasmine Rice (5lb)", Price = 7.99m, CategoryId = pantry.Id, StockQuantity = 80, WeightGrams = 2268, Description = "Fragrant long-grain jasmine rice." },
            new() { Sku = "PANT-003", Name = "Diced Tomatoes (14.5oz)", Price = 1.29m, CategoryId = pantry.Id, StockQuantity = 300, WeightGrams = 411, Description = "Fire-roasted diced tomatoes." },
            new() { Sku = "PANT-004", Name = "Extra Virgin Olive Oil (16oz)", Price = 11.99m, CategoryId = pantry.Id, ManufacturerId = organic.Id, StockQuantity = 45, WeightGrams = 473, Description = "Cold-pressed extra virgin olive oil." },

            // Beverages
            new() { Sku = "BEVE-001", Name = "Orange Juice (52oz)", Price = 5.49m, CategoryId = beverages.Id, StockQuantity = 70, WeightGrams = 1542, Description = "100% pure squeezed orange juice." },
            new() { Sku = "BEVE-002", Name = "Sparkling Water (12pk)", Price = 8.99m, CategoryId = beverages.Id, StockQuantity = 90, WeightGrams = 4320, Description = "Unflavored sparkling mineral water." },
            new() { Sku = "BEVE-003", Name = "Green Tea (20 bags)", Price = 4.29m, CategoryId = beverages.Id, ManufacturerId = organic.Id, StockQuantity = 110, WeightGrams = 40, Description = "Organic green tea bags." },

            // Frozen
            new() { Sku = "FROZ-001", Name = "Frozen Organic Blueberries (10oz)", Price = 4.99m, CategoryId = frozen.Id, ManufacturerId = organic.Id, StockQuantity = 65, WeightGrams = 283, Description = "Flash-frozen wild blueberries." },
            new() { Sku = "FROZ-002", Name = "Frozen Pizza (cheese)", Price = 6.99m, CategoryId = frozen.Id, StockQuantity = 40, WeightGrams = 510, Description = "Classic four-cheese frozen pizza." },
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        // Product images (Unsplash free photos)
        var productImages = new List<ProductImage>
        {
            // Produce
            new() { ProductId = products[0].Id,  Url = "https://images.unsplash.com/photo-1571771894821-ce9b6c11b08e?w=800&q=80", AltText = "Organic Bananas",              DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[1].Id,  Url = "https://images.unsplash.com/photo-1464965911861-746a04b4bca6?w=800&q=80", AltText = "Fresh Strawberries",          DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[2].Id,  Url = "https://images.unsplash.com/photo-1576045057995-568f588f82fb?w=800&q=80", AltText = "Baby Spinach",                 DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[3].Id,  Url = "https://images.unsplash.com/photo-1523049673857-eb18f1d7b578?w=800&q=80", AltText = "Avocados",                     DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[4].Id,  Url = "https://images.unsplash.com/photo-1607305387299-a3d9611cd469?w=800&q=80", AltText = "Roma Tomatoes",                DisplayOrder = 0, IsPrimary = true },

            // Dairy
            new() { ProductId = products[5].Id,  Url = "https://images.unsplash.com/photo-1563636619-e9143da7973b?w=800&q=80", AltText = "Whole Milk",                    DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[6].Id,  Url = "https://images.unsplash.com/photo-1582722872445-44dc5f7e3c8f?w=800&q=80", AltText = "Free-Range Eggs",             DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[7].Id,  Url = "https://images.unsplash.com/photo-1552767059-ce182ead6c1b?w=800&q=80", AltText = "Sharp Cheddar Cheese",         DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[8].Id,  Url = "https://images.unsplash.com/photo-1488477181946-6428a0291777?w=800&q=80", AltText = "Greek Yogurt",                DisplayOrder = 0, IsPrimary = true },

            // Bakery
            new() { ProductId = products[9].Id,  Url = "https://images.unsplash.com/photo-1586444248902-2f64eddc13df?w=800&q=80", AltText = "Sourdough Bread Loaf",       DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[10].Id, Url = "https://images.unsplash.com/photo-1607958996333-41aef7caefaa?w=800&q=80", AltText = "Blueberry Muffins",           DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[11].Id, Url = "https://images.unsplash.com/photo-1651248341193-a4035e081fbc?w=800&q=80", AltText = "Whole Wheat Bagels",          DisplayOrder = 0, IsPrimary = true },

            // Meat & Seafood
            new() { ProductId = products[12].Id, Url = "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800&q=80", AltText = "Chicken Breast",              DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[13].Id, Url = "https://images.unsplash.com/photo-1602470520998-f4a52199a3d6?w=800&q=80", AltText = "Ground Beef",                 DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[14].Id, Url = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=800&q=80", AltText = "Atlantic Salmon Fillet",      DisplayOrder = 0, IsPrimary = true },

            // Pantry
            new() { ProductId = products[15].Id, Url = "https://images.unsplash.com/photo-1551462147-ff29053bfc14?w=800&q=80", AltText = "Organic Pasta",               DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[16].Id, Url = "https://images.unsplash.com/photo-1586201375761-83865001e31c?w=800&q=80", AltText = "Jasmine Rice",                DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[17].Id, Url = "https://images.unsplash.com/photo-1558818498-28c1e002b655?w=800&q=80", AltText = "Diced Tomatoes",              DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[18].Id, Url = "https://images.unsplash.com/photo-1474979266404-7eaacbcd87c5?w=800&q=80", AltText = "Extra Virgin Olive Oil",      DisplayOrder = 0, IsPrimary = true },

            // Beverages
            new() { ProductId = products[19].Id, Url = "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800&q=80", AltText = "Orange Juice",               DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[20].Id, Url = "https://images.unsplash.com/photo-1613424168901-b69f533bfb11?w=800&q=80", AltText = "Sparkling Water",             DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[21].Id, Url = "https://images.unsplash.com/photo-1627435601361-ec25f5b1d0e5?w=800&q=80", AltText = "Green Tea",                  DisplayOrder = 0, IsPrimary = true },

            // Frozen
            new() { ProductId = products[22].Id, Url = "https://images.unsplash.com/photo-1498557850523-fd3d118b962e?w=800&q=80", AltText = "Frozen Organic Blueberries", DisplayOrder = 0, IsPrimary = true },
            new() { ProductId = products[23].Id, Url = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800&q=80", AltText = "Frozen Pizza",               DisplayOrder = 0, IsPrimary = true },
        };

        db.ProductImages.AddRange(productImages);
        await db.SaveChangesAsync();

        // Active sale — 20% off Produce category
        var produceSale = new Sale
        {
            Name = "Fresh Produce Sale",
            Description = "20% off all fresh produce!",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(7),
            DiscountType = "Percentage",
            DiscountValue = 20,
            IsActive = true
        };

        db.Sales.Add(produceSale);
        await db.SaveChangesAsync();

        db.SaleCategories.Add(new SaleCategory { SaleId = produceSale.Id, CategoryId = produce.Id });
        await db.SaveChangesAsync();
    }
}
