using Dapper;
using Microsoft.Data.SqlClient;
using ShopNest.Data;
using ShopNest.Models;
using ShopNest.Repositories.Interfaces;
using System.Data;

namespace ShopNest.Repositories.Implementations
{
    // Repositories/Implementations/ProductRepository.cs
    public class ProductRepository : GenericRepository<Product>,
                                      IProductRepository
    {
        private readonly IDbConnection _db;

        public ProductRepository(AppDbContext context,
                                 IConfiguration config) : base(context)
        {
            _db = new SqlConnection(
                config.GetConnectionString("DefaultConnection"));
        }

        // ✅ Dapper → Fast Search
        public async Task<List<Product>> SearchByNameAsync(string keyword)
            => (await _db.QueryAsync<Product>(
                "SELECT * FROM Products WHERE Name LIKE @kw",
                new { kw = $"%{keyword}%" })).ToList();

        // ✅ Dapper → Category Filter
        public async Task<List<Product>> GetByCategoryAsync(int categoryId)
            => (await _db.QueryAsync<Product>(
                "SELECT * FROM Products WHERE CategoryId = @id",
                new { id = categoryId })).ToList();

        public Task<List<Product>> GetLowStockAsync(int threshold)
        {
            throw new NotImplementedException();
        }
    }
}
