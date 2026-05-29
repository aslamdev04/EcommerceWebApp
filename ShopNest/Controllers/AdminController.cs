using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShopNest.Models;
using ShopNest.Models.ViewModels;
using ShopNest.Repositories.Interfaces;

namespace ShopNest.Controllers
{
    [Authorize(Roles ="Admin")]
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
            var categories = await _uow.Categories.GetAllAsync();

            // Dropdown ke liye — Icon + Name saath dikhao
            ViewBag.Categories = new SelectList(
                categories.Select(c => new {
                    c.CategoryId,
                    DisplayName = $"{c.Icon} {c.CategoryName}"
                }),
                "CategoryId",
                "DisplayName"
            );

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
        public IActionResult user_mgmt()
        {
            return View();
        }
        #endregion

        #region Order Management
        public IActionResult Order_mgmt()
        {
            return View();
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
        public IActionResult Reports()
        {
            return View();
        }
     
    }
}
