using ShopNest.Models;

namespace ShopNest.Repositories.Interfaces
{
    // Repositories/Interfaces/IProductRepository.cs
    public interface IProductRepository : IGenericRepository<Product>
    {
        // Dapper → Complex Queries
        Task<List<Product>> SearchByNameAsync(string keyword);
        Task<List<Product>> GetByCategoryAsync(int categoryId);
        Task<List<Product>> GetLowStockAsync(int threshold);
    }
}
