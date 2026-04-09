using AprilPractice.Application.Interfaces;
using AprilPractice.Domain.Entities;
using AprilPractice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AprilPractice.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetCategoryWithProductsAsync(int id)
    {
        return await _dbSet.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
    }
}
