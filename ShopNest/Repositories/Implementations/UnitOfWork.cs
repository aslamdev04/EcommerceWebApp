using ShopNest.Data;
using ShopNest.Models;
using ShopNest.Repositories.Interfaces;

namespace ShopNest.Repositories.Implementations
{
    // Repositories/Implementations/UnitOfWork.cs
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IGenericRepository<Product> Products { get; private set; }
        public IGenericRepository<Order> Orders { get; private set; }
        public IGenericRepository<Cart> Carts { get; private set; }
        public IProductRepository ProductSearch { get; private set; }
        public IGenericRepository<User> Users { get; private set; }        // ← Add
        public IGenericRepository<OrderItem> OrderItems { get; private set; } // ← Add
        public IGenericRepository<Category> Categories { get; private set; }

        public UnitOfWork(AppDbContext context, IConfiguration config)
        {
            _context = context;
            Products = new GenericRepository<Product>(context);
            Orders = new GenericRepository<Order>(context);
            Carts = new GenericRepository<Cart>(context);
            ProductSearch = new ProductRepository(context, config);
            Users = new GenericRepository<User>(context);        // ← Add
            OrderItems = new GenericRepository<OrderItem>(context);   // ← Add
            Categories = new GenericRepository<Category>(context);
        }

        public async Task<int> SaveAsync()
            => await _context.SaveChangesAsync(); // ← Sirf ek baar!

        public void Dispose() => _context.Dispose();
    }
}
