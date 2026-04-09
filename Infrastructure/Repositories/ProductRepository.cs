using AprilPractice.Application.Interfaces;
using AprilPractice.Domain.Entities;
using AprilPractice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AprilPractice.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetProductsWithCategoryAsync()
    {
        return await _dbSet.Include(p => p.Category).ToListAsync();
    }

    public async Task<Product?> GetProductWithCategoryAsync(int id)
    {
        return await _dbSet.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }
}
