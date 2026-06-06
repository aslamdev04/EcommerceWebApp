using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Models;
using ShopNest.Models.ViewModels;
using ShopNest.Repositories.Interfaces;
using System.Diagnostics;

namespace ShopNest.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _uow;

        public HomeController(ILogger<HomeController> logger,
                              IUnitOfWork uow)
        {
            _logger = logger;
            _uow = uow;
        }

        // ── Landing Page ──────────────────────────────
        public async Task<IActionResult> Index()
        {
            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();

            // Featured products — latest 8
            var featured = products
                .OrderByDescending(p => p.ProductId)
                .Take(8)
                .Select(p => {
                    var cat = categories
                        .FirstOrDefault(c => c.CategoryId == p.CategoryId);
                    return new ProductBrowseViewModel
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

            // Categories for homepage
            ViewBag.Categories = categories
                .Where(c => c.IsActive)
                .ToList();

            ViewBag.TotalProducts = products.Count;

            return View(featured);
        }

        // ── Public Products Page ──────────────────────
        public async Task<IActionResult> Products(
                                int? categoryId,
                                string search = "",
                                string sort = "popular")
        {
            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();

            // Search filter
            if (!string.IsNullOrEmpty(search))
                products = products
                    .Where(p => p.Name.ToLower()
                    .Contains(search.ToLower()))
                    .ToList();

            // Category filter
            if (categoryId.HasValue)
                products = products
                    .Where(p => p.CategoryId == categoryId)
                    .ToList();

            // Sort
            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                _ => products.OrderByDescending(p => p.ProductId).ToList()
            };

            var viewModel = products.Select(p => {
                var cat = categories
                    .FirstOrDefault(c => c.CategoryId == p.CategoryId);
                return new ProductBrowseViewModel
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

            ViewBag.Categories = categories.Where(c => c.IsActive).ToList();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedSort = sort;
            ViewBag.Search = search;

            return View(viewModel);
        }

        // ── Search Page ───────────────────────────────
        public async Task<IActionResult> Search(string q = "")
        {
            if (string.IsNullOrEmpty(q))
                return View(new List<ProductBrowseViewModel>());

            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();

            var results = products
                .Where(p => p.Name.ToLower().Contains(q.ToLower()) ||
                            p.Description != null &&
                            p.Description.ToLower().Contains(q.ToLower()))
                .Select(p => {
                    var cat = categories
                        .FirstOrDefault(c => c.CategoryId == p.CategoryId);
                    return new ProductBrowseViewModel
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

            ViewBag.Query = q;
            ViewBag.ResultCount = results.Count;

            return View(results);
        }

        // ── About Page ────────────────────────────────
        public IActionResult About() => View();

        // ── Contact Page ─────────────────────────────
        public IActionResult Contacts() => View();

        // ── Error Page ────────────────────────────────
        [ResponseCache(Duration = 0,
                       Location = ResponseCacheLocation.None,
                       NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                         ?? HttpContext.TraceIdentifier
            });
        }
    }
}