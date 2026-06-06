using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopNest.Models;
using ShopNest.Models.ViewModels;
using ShopNest.Repositories.Interfaces;

namespace ShopNest.Controllers
{
    //[Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public AdminController(IUnitOfWork uow,
                               IWebHostEnvironment env)
        {
            _uow = uow;
            _env = env;
        }

        #region Admin Dashboard
        public async Task<IActionResult> Index()
        {
            // ── Categories dropdown ───────────────────────────
            var categories = await _uow.Categories.GetAllAsync();
            ViewBag.Categories = new SelectList(
                categories.Select(c => new {
                    c.CategoryId,
                    DisplayName = $"{c.Icon} {c.CategoryName}"
                }),
                "CategoryId",
                "DisplayName"
            );

            // ── Real Stats ────────────────────────────────────
            var allUsers = await _uow.Users.GetAllAsync();
            var allProducts = await _uow.Products.GetAllAsync();
            var allOrders = await _uow.Orders.GetAllAsync();

            // Stats Cards
            ViewBag.TotalUsers = allUsers.Count(u => u.Role == "User");
            ViewBag.TotalProducts = allProducts.Count;
            ViewBag.TotalOrders = allOrders.Count;
            ViewBag.TotalRevenue = allOrders
                .Where(o => o.Status != "Cancelled")
                .Sum(o => o.TotalAmount);

            // Low Stock
            ViewBag.LowStockCount = allProducts.Count(p => p.Stock > 0 && p.Stock <= 10);

            // Orders by Status
            ViewBag.PendingOrders = allOrders.Count(o => o.Status == "Pending");
            ViewBag.ProcessingOrders = allOrders.Count(o => o.Status == "Processing");
            ViewBag.ShippedOrders = allOrders.Count(o => o.Status == "Shipped");
            ViewBag.DeliveredOrders = allOrders.Count(o => o.Status == "Delivered");
            ViewBag.CancelledOrders = allOrders.Count(o => o.Status == "Cancelled");

            // Monthly Revenue — Last 6 months
            var monthlyRevenue = new List<object>();
            for (int i = 5; i >= 0; i--)
            {
                var month = DateTime.UtcNow.AddMonths(-i);
                var revenue = allOrders
                    .Where(o => o.OrderDate.Month == month.Month
                             && o.OrderDate.Year == month.Year
                             && o.Status != "Cancelled")
                    .Sum(o => o.TotalAmount);
                var orders = allOrders
                    .Count(o => o.OrderDate.Month == month.Month
                             && o.OrderDate.Year == month.Year);

                monthlyRevenue.Add(new
                {
                    Month = month.ToString("MMM"),
                    Revenue = revenue,
                    Orders = orders
                });
            }
            ViewBag.MonthlyRevenue = monthlyRevenue;

            // Recent Activity — Last 5 orders
            var allUsers2 = await _uow.Users.GetAllAsync();
            ViewBag.RecentOrders = allOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new {
                    OrderId = o.OrderId,
                    CustomerName = allUsers2
                        .FirstOrDefault(u => u.UserId == o.UserId)?.Name ?? "Unknown",
                    Amount = o.TotalAmount,
                    Status = o.Status,
                    Date = o.OrderDate.ToString("MMM dd")
                }).ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddProduct(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Validation fail — categories reload karo dropdown ke liye
                var cats = await _uow.Categories.GetAllAsync();
                ViewBag.Categories = new SelectList(
                    cats.Select(c => new {
                        c.CategoryId,
                        DisplayName = $"{c.Icon} {c.CategoryName}"
                    }),
                    "CategoryId",
                    "DisplayName"
                );
                TempData["Error"] = "Please fill all required fields!";
                return View("Index", model);
            }

            // ── Image Save Karo ───────────────────────────────
            string imageUrl = "/images/default.png"; // Default image

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                // Folder banao agar nahi hai
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                // Unique file name banao
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // File save karo
                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                imageUrl = $"/images/products/{fileName}";
            }

            // ── Product Object Banao ──────────────────────────
            var product = new ShopNest.Models.Product
            {
                Name = model.Name,
                CategoryId = model.CategoryId,
                Price = model.Price,
                Stock = model.Stock,
                Description = model.Description,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            // ── DB Mein Save Karo ─────────────────────────────
            await _uow.Products.AddAsync(product);
            await _uow.SaveAsync();

            TempData["Success"] = $"✅ Product '{product.Name}' added successfully!";
            return RedirectToAction("Index");
        }
        #endregion

        #region Product Monitoring 
        public async Task<IActionResult> Product_mon()
        {
            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();

            var viewModel = products.Select(p => {
                var cat = categories.FirstOrDefault(c => c.CategoryId == p.CategoryId);
                return new ProductMonViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    CategoryName = cat?.CategoryName ?? "N/A",
                    CategoryIcon = cat?.Icon ?? "📦",
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl
                };
            }).ToList();

            // Stats ke liye ViewBag
            ViewBag.TotalProducts = viewModel.Count;
            ViewBag.InStock = viewModel.Count(p => p.Stock > 10);
            ViewBag.LowStock = viewModel.Count(p => p.Stock > 0 && p.Stock <= 10);
            ViewBag.OutOfStock = viewModel.Count(p => p.Stock == 0);

            // Categories dropdown filter ke liye
            ViewBag.Categories = categories.Select(c => c.CategoryName).ToList();

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _uow.Products.DeleteAsync(id);
            await _uow.SaveAsync();

            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction("Product_mon");
        }
     
        // GET — Edit Form Open Karo
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _uow.Categories.GetAllAsync();
            ViewBag.Categories = new SelectList(
                categories.Select(c => new {
                    c.CategoryId,
                    DisplayName = $"{c.Icon} {c.CategoryName}"
                }),
                "CategoryId",
                "DisplayName"
            );

            // Product data form mein load karo
            var model = new ProductViewModel
            {
                Name = product.Name,
                CategoryId = product.CategoryId,
                Price = product.Price,
                Stock = product.Stock,
                Description = product.Description,

            };

            ViewBag.ProductId = id;
            ViewBag.ExistingImage = product.ImageUrl;

            return View(model);
        }

        // POST — Edit Form Submit
        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cats = await _uow.Categories.GetAllAsync();
                ViewBag.Categories = new SelectList(
                    cats.Select(c => new {
                        c.CategoryId,
                        DisplayName = $"{c.Icon} {c.CategoryName}"
                    }),
                    "CategoryId",
                    "DisplayName"
                );
                ViewBag.ProductId = id;
                return View(model);
            }

            var product = await _uow.Products.GetByIdAsync(id);
            if (product == null) return NotFound();

            // ── Nai Image Upload Karo (agar di hai) ──────────
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath,
                                                 "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}" +
                               $"{Path.GetExtension(model.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await model.ImageFile.CopyToAsync(stream);

                product.ImageUrl = $"/images/products/{fileName}";
            }

            // ── Fields Update Karo ────────────────────────────
            product.Name = model.Name;
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;
            product.Stock = model.Stock;
            product.Description = model.Description;

            await _uow.Products.UpdateAsync(product);
            await _uow.SaveAsync();

            TempData["Success"] = $"✅ '{product.Name}' updated successfully!";
            return RedirectToAction("Product_mon");
        }
        #endregion

        #region Category Management
        // GET — Category List
        public async Task<IActionResult> Cat_mgmt()
        {
            var categories = await _uow.Categories.GetAllAsync();
            var products = await _uow.Products.GetAllAsync();

            // Har category ke liye product count
            ViewBag.ProductCounts = products
       .Where(p => p.CategoryId != null)
       .GroupBy(p => p.CategoryId!.Value)    // ← .Value use karo
       .ToDictionary(g => g.Key, g => g.Count());

            return View(categories);
        }

        // POST — Add Category
        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill all fields!";
                return RedirectToAction("Cat_mgmt");
            }

            // Duplicate check
            var existing = await _uow.Categories.GetAllAsync();
            if (existing.Any(c => c.CategoryName.ToLower() == model.CategoryName.ToLower()))
            {
                TempData["Error"] = "Category already exists!";
                return RedirectToAction("Cat_mgmt");
            }

            var category = new Category
            {
                CategoryName = model.CategoryName,
                Icon = model.Icon,
                IsActive = model.IsActive
            };

            await _uow.Categories.AddAsync(category);
            await _uow.SaveAsync();

            TempData["Success"] = $"{model.Icon} '{model.CategoryName}' added!";
            return RedirectToAction("Cat_mgmt");
        }

        // POST — Toggle Active/Inactive
        [HttpPost]
        public async Task<IActionResult> ToggleCategory(int id)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return NotFound();

            category.IsActive = !category.IsActive;
            await _uow.Categories.UpdateAsync(category);
            await _uow.SaveAsync();

            TempData["Success"] = $"'{category.CategoryName}' " +
                                  $"{(category.IsActive ? "activated" : "deactivated")}!";
            return RedirectToAction("Cat_mgmt");
        }

        // POST — Delete Category
        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _uow.Categories.GetByIdAsync(id);
            if (category == null) return NotFound();

            // Products hain toh delete mat karo
            var products = await _uow.Products.GetAllAsync();
            if (products.Any(p => p.CategoryId == id))
            {
                TempData["Error"] = $"Cannot delete '{category.CategoryName}' — products exist!";
                return RedirectToAction("Cat_mgmt");
            }

            await _uow.Categories.DeleteAsync(id);
            await _uow.SaveAsync();

            TempData["Success"] = $"'{category.CategoryName}' deleted!";
            return RedirectToAction("Cat_mgmt");
        }
        #endregion

        #region UserManagemt
        public async Task<IActionResult> User_mgmt(string role = "All",
                                             string status = "All")
        {
            var allUsers = await _uow.Users.GetAllAsync();
            var allOrders = await _uow.Orders.GetAllAsync();

            // Sirf customers dikhao (Admin nahi)
            var users = allUsers.Where(u => u.Role != "Admin").ToList();

            // Role filter
            if (role != "All")
                users = users.Where(u => u.Role == role).ToList();

            // Status filter
            if (status == "Active")
                users = users.Where(u => u.IsActive).ToList();
            else if (status == "Inactive")
                users = users.Where(u => !u.IsActive).ToList();

            var viewModel = users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => {
                    var userOrders = allOrders
                        .Where(o => o.UserId == u.UserId).ToList();
                    return new UserMgmtViewModel
                    {
                        UserId = u.UserId,
                        Name = u.Name,
                        Email = u.Email,
                        Phone = u.Phone ?? "N/A",
                        City = string.IsNullOrEmpty(u.City)
                                      ? "N/A" : u.City,
                        CreatedAt = u.CreatedAt,
                        Role = u.Role,
                        IsActive = u.IsActive,
                        TotalOrders = userOrders.Count,
                        TotalSpent = userOrders.Sum(o => o.TotalAmount)
                    };
                }).ToList();

            // Stats ke liye
            ViewBag.TotalUsers = users.Count;
            ViewBag.ActiveUsers = users.Count(u => u.IsActive);
            ViewBag.InactiveUsers = users.Count(u => !u.IsActive);
            ViewBag.SelectedRole = role;
            ViewBag.SelectedStatus = status;

            return View(viewModel);
        }

        // POST — Toggle Active/Inactive
        [HttpPost]
        public async Task<IActionResult> ToggleUser(int id)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _uow.Users.UpdateAsync(user);
            await _uow.SaveAsync();

            TempData["Success"] = $"'{user.Name}' " +
                                  $"{(user.IsActive ? "activated" : "deactivated")}!";
            return RedirectToAction("User_mgmt");
        }

        // POST — Change Role
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int id, string role)
        {
            var user = await _uow.Users.GetByIdAsync(id);
            if (user == null) return NotFound();

            user.Role = role;
            await _uow.Users.UpdateAsync(user);
            await _uow.SaveAsync();

            TempData["Success"] = $"'{user.Name}' role changed to {role}!";
            return RedirectToAction("User_mgmt");
        }
        #endregion

        #region Order Management
        public async Task<IActionResult> Order_mgmt(string status = "All")
        {
            var allOrders = await _uow.Orders.GetAllAsync();
            var allOrderItems = await _uow.OrderItems.GetAllAsync();
            var allUsers = await _uow.Users.GetAllAsync();

            // Status filter
            var filteredOrders = status == "All"
                ? allOrders
                : allOrders.Where(o => o.Status == status).ToList();

            var viewModel = filteredOrders
                .OrderByDescending(o => o.OrderDate)
                .Select(o => {
                    var user = allUsers.FirstOrDefault(u => u.UserId == o.UserId);
                    var items = allOrderItems.Count(oi => oi.OrderId == o.OrderId);
                    var name = user?.Name ?? "Unknown";
                    var initials = name.Length >= 2
                        ? $"{name[0]}{name.Split(' ').LastOrDefault()?[0]}"
                                    .ToUpper()
                        : name.Substring(0, 1).ToUpper();

                    return new OrderMgmtViewModel
                    {
                        OrderId = o.OrderId,
                        CustomerName = name,
                        CustomerInitials = initials,
                        ItemCount = items,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        Status = o.Status
                    };
                }).ToList();

            // Stats ke liye
            ViewBag.PendingCount = allOrders.Count(o => o.Status == "Pending");
            ViewBag.ProcessingCount = allOrders.Count(o => o.Status == "Processing");
            ViewBag.ShippedCount = allOrders.Count(o => o.Status == "Shipped");
            ViewBag.DeliveredCount = allOrders.Count(o => o.Status == "Delivered");
            ViewBag.CancelledCount = allOrders.Count(o => o.Status == "Cancelled");
            ViewBag.SelectedStatus = status;

            return View(viewModel);
        }

        // POST — Status Update
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            order.Status = status;
            await _uow.Orders.UpdateAsync(order);
            await _uow.SaveAsync();

            TempData["Success"] = $"Order #ORD-{orderId:D4} → {status}!";
            return RedirectToAction("Order_mgmt");
        }
        #endregion

        #region Report Action
        public async Task<IActionResult> Reports()
        {
            var allOrders = await _uow.Orders.GetAllAsync();
            var allOrderItems = await _uow.OrderItems.GetAllAsync();
            var allProducts = await _uow.Products.GetAllAsync();
            var allCategories = await _uow.Categories.GetAllAsync();
            var allUsers = await _uow.Users.GetAllAsync();

            var now = DateTime.UtcNow;
            var thisMonth = allOrders
                .Where(o => o.OrderDate.Month == now.Month &&
                            o.OrderDate.Year == now.Year)
                .ToList();

            // ── Stats ─────────────────────────────────────────
            int newUsers = allUsers.Count(u =>
                            u.CreatedAt.Month == now.Month &&
                            u.CreatedAt.Year == now.Year &&
                            u.Role == "User");

            int ordersCount = thisMonth.Count;

            decimal revenueThisMonth = thisMonth
                .Where(o => o.Status != "Cancelled")
                .Sum(o => o.TotalAmount);

            // ← Fix 1: ToString without format parameter
            string revenueK = Math.Round(revenueThisMonth / 1000m, 1).ToString();

            int totalOrders = allOrders.Count;
            int cancelledOrders = allOrders.Count(o => o.Status == "Cancelled");
            double returnRate = totalOrders > 0
                ? Math.Round((cancelledOrders / (double)totalOrders) * 100, 1)
                : 0;

            // ── Top Categories ────────────────────────────────
            var catRevenue = allOrderItems
                .Join(allProducts,
                      oi => oi.ProductId,
                      p => p.ProductId,
                      (oi, p) => new
                      {
                          CategoryId = p.CategoryId,
                          Revenue = oi.UnitPrice * oi.Quantity
                      })
                .GroupBy(x => x.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Revenue = (decimal?)g.Sum(x => x.Revenue) // ← nullable
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();

            // ← Fix 2: Nullable decimal fix
            decimal maxRev = 1m;
            if (catRevenue.Any())
            {
                var maxVal = catRevenue.Max(x => x.Revenue);
                if (maxVal.HasValue && maxVal.Value > 0)
                    maxRev = maxVal.Value;
            }

            var categoryRevenueList = catRevenue.Select(x => {
                var cat = allCategories
                    .FirstOrDefault(c => c.CategoryId == x.CategoryId);
                decimal rev = x.Revenue ?? 0m;
                return new CategoryRevenueViewModel
                {
                    CategoryName = cat?.CategoryName ?? "N/A",
                    Icon = cat?.Icon ?? "📦",
                    RevenueK = Math.Round(rev / 1000m, 1).ToString(),
                    Percentage = (int)((rev / maxRev) * 100)
                };
            }).ToList();

            // ── Top Products ──────────────────────────────────
            var topProductsList = allOrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSales = g.Sum(oi => oi.Quantity),
                    TotalRevenue = (decimal?)g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(5)
                .ToList();

            var topProducts = topProductsList.Select(x => {
                var product = allProducts
                    .FirstOrDefault(p => p.ProductId == x.ProductId);
                var category = allCategories
                    .FirstOrDefault(c => c.CategoryId == product?.CategoryId);
                decimal rev = x.TotalRevenue ?? 0m;
                return new TopProductViewModel
                {
                    Name = product?.Name ?? "N/A",
                    Icon = category?.Icon ?? "📦",
                    TotalSales = x.TotalSales,
                    RevenueK = Math.Round(rev / 1000m, 1).ToString()
                };
            }).ToList();

            // ── ViewModel ─────────────────────────────────────
            var viewModel = new ReportsViewModel
            {
                NewUsersThisMonth = newUsers,
                OrdersThisMonth = ordersCount,
                RevenueThisMonthK = revenueK,
                ReturnRate = returnRate,
                CategoryRevenue = categoryRevenueList,
                TopProducts = topProducts
            };

            return View(viewModel);
        }
        #endregion
        public IActionResult Payment_mgmt()
        {
            return View();
        }
        public IActionResult Offer_noti()
        {
            return View();
        }
     
     
    }
}
