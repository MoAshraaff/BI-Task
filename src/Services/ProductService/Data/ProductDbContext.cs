using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Category).HasMaxLength(80);
            // SQLite has no native decimal type and cannot ORDER BY a TEXT-mapped decimal
            // column, which breaks OData $orderby. Store price as REAL instead.
            entity.Property(p => p.Price).HasConversion<double>();
        });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Category = "Accessories", Price = 19.99m, Stock = 150, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Mechanical Keyboard", Description = "RGB backlit mechanical keyboard", Category = "Accessories", Price = 79.99m, Stock = 75, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "27-inch Monitor", Description = "4K UHD monitor", Category = "Displays", Price = 329.00m, Stock = 40, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 4, Name = "USB-C Hub", Description = "7-in-1 USB-C hub", Category = "Accessories", Price = 34.50m, Stock = 200, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 5, Name = "Laptop Stand", Description = "Aluminum adjustable laptop stand", Category = "Furniture", Price = 45.00m, Stock = 90, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
