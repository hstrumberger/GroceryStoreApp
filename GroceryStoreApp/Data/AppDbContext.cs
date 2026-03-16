using GroceryStoreApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GroceryStoreApp.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleProduct> SaleProducts => Set<SaleProduct>();
    public DbSet<SaleCategory> SaleCategories => Set<SaleCategory>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User
        builder.Entity<User>(e =>
        {
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
        });

        // Category
        builder.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Slug).HasMaxLength(100);
            e.HasIndex(c => c.Slug).IsUnique();
            e.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Manufacturer
        builder.Entity<Manufacturer>(e =>
        {
            e.Property(m => m.Name).HasMaxLength(200);
        });

        // Product
        builder.Entity<Product>(e =>
        {
            e.Property(p => p.Sku).HasMaxLength(100);
            e.HasIndex(p => p.Sku).IsUnique();
            e.Property(p => p.Name).HasMaxLength(500);
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.WeightGrams).HasColumnType("decimal(10,3)");
            e.Property(p => p.DimensionLengthCm).HasColumnType("decimal(10,2)");
            e.Property(p => p.DimensionWidthCm).HasColumnType("decimal(10,2)");
            e.Property(p => p.DimensionHeightCm).HasColumnType("decimal(10,2)");
            e.Property(p => p.AverageRating).HasColumnType("decimal(3,2)");
        });

        // ProductImage
        builder.Entity<ProductImage>(e =>
        {
            e.HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Sale
        builder.Entity<Sale>(e =>
        {
            e.Property(s => s.DiscountType).HasMaxLength(20);
            e.Property(s => s.DiscountValue).HasColumnType("decimal(18,2)");
        });

        // SaleProduct composite PK
        builder.Entity<SaleProduct>(e =>
        {
            e.HasKey(sp => new { sp.SaleId, sp.ProductId });
            e.HasOne(sp => sp.Sale).WithMany(s => s.SaleProducts).HasForeignKey(sp => sp.SaleId);
            e.HasOne(sp => sp.Product).WithMany(p => p.SaleProducts).HasForeignKey(sp => sp.ProductId);
        });

        // SaleCategory composite PK
        builder.Entity<SaleCategory>(e =>
        {
            e.HasKey(sc => new { sc.SaleId, sc.CategoryId });
            e.HasOne(sc => sc.Sale).WithMany(s => s.SaleCategories).HasForeignKey(sc => sc.SaleId);
            e.HasOne(sc => sc.Category).WithMany(c => c.SaleCategories).HasForeignKey(sc => sc.CategoryId);
        });

        // Cart
        builder.Entity<Cart>(e =>
        {
            e.HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CartItem
        builder.Entity<CartItem>(e =>
        {
            e.HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
        });

        // Order
        builder.Entity<Order>(e =>
        {
            e.Property(o => o.Status).HasMaxLength(50);
            e.Property(o => o.SubTotal).HasColumnType("decimal(18,2)");
            e.Property(o => o.DiscountAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.TaxAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.ShippingAmount).HasColumnType("decimal(18,2)");
            e.Property(o => o.Total).HasColumnType("decimal(18,2)");
            e.Property(o => o.PaymentLast4).HasMaxLength(4);
        });

        // OrderItem
        builder.Entity<OrderItem>(e =>
        {
            e.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.DiscountedPrice).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");
        });

        // RefreshToken
        builder.Entity<RefreshToken>(e =>
        {
            e.Property(rt => rt.Token).HasMaxLength(500);
            e.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
