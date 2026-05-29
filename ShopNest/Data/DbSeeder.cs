using Microsoft.AspNetCore.Identity;
using ShopNest.Models;
using ShopNest.Repositories.Interfaces;

namespace ShopNest.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var uow = services.GetRequiredService<IUnitOfWork>();
            var hasher = new PasswordHasher<User>();

            // ── Admin Seed ─────────────────────────────────
            var users = await uow.Users.GetAllAsync();
            if (!users.Any(u => u.Role == "Admin"))
            {
                var admin = new User
                {
                    Name = "Admin",
                    Email = "admin@shopnest.com",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
                await uow.Users.AddAsync(admin);
                await uow.SaveAsync();
            }

            // ── Categories Seed ────────────────────────────
            var categories = await uow.Categories.GetAllAsync();
            if (!categories.Any())
            {
                var defaultCategories = new List<Category>
                {
                    new Category { CategoryName = "Electronics",    Icon = "📱" },
                    new Category { CategoryName = "Fashion",        Icon = "👗" },
                    new Category { CategoryName = "Home & Living",  Icon = "🏠" },
                    new Category { CategoryName = "Beauty",         Icon = "💄" },
                    new Category { CategoryName = "Books",          Icon = "📚" },
                    new Category { CategoryName = "Sports",         Icon = "🏋️" },
                    new Category { CategoryName = "Food & Grocery", Icon = "🍕" },
                };

                foreach (var cat in defaultCategories)
                {
                    await uow.Categories.AddAsync(cat);
                }
                await uow.SaveAsync();
            }
        } // ← SeedAdminAsync end
    } // ← DbSeeder end
} // ← namespace end