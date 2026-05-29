using ShopNest.Models;

namespace ShopNest.Repositories.Interfaces
{
    // Repositories/Interfaces/IUnitOfWork.cs
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<Cart> Carts { get; }
        IGenericRepository<User> Users { get; }        // ← Yeh add karo
        IGenericRepository<OrderItem> OrderItems { get; } // ← Yeh bhi
        IGenericRepository<Category> Categories { get; }
        IProductRepository ProductSearch { get; }
        Task<int> SaveAsync();
    }
}
