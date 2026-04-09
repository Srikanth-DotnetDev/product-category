using AprilPractice.Domain.Entities;

namespace AprilPractice.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetCategoryWithProductsAsync(int id);
}
