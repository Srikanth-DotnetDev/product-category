using AprilPractice.Domain.Entities;

namespace AprilPractice.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
    Task<Product?> GetProductWithCategoryAsync(int id);
}
