using AprilPractice.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AprilPractice.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and gadgets" },
            new Category { Id = 2, Name = "Clothing", Description = "Apparel and accessories" },
            new Category { Id = 3, Name = "Books", Description = "Physical and digital books" }
        );

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, Stock = 50, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Smartphone", Description = "Latest smartphone model", Price = 699.99m, Stock = 100, CategoryId = 1, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "T-Shirt", Description = "Cotton t-shirt", Price = 29.99m, Stock = 200, CategoryId = 2, CreatedAt = DateTime.UtcNow },
            new Product { Id = 4, Name = "Programming Book", Description = "Learn C# programming", Price = 49.99m, Stock = 75, CategoryId = 3, CreatedAt = DateTime.UtcNow }
        );
    }
}
