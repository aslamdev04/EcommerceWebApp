using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Models;
using ShopNest.Models.ViewModels;
using ShopNest.Repositories.Interfaces;
using System.Security.Claims;

namespace ShopNest.Controllers
{
    [Authorize(Roles = "User")]
 
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork _uow;

        public CustomerController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            // Logged in user ka ID lo JWT token se
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            // User info
            var user = await _uow.Users.GetByIdAsync(userId);

            // User ke saare orders
            var allOrders = await _uow.Orders.GetAllAsync();
            var userOrders = allOrders.Where(o => o.UserId == userId).ToList();

            // OrderItems
            var allOrderItems = await _uow.OrderItems.GetAllAsync();

            // Recent 3 orders
            var recentOrders = userOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    ItemCount = allOrderItems.Count(oi => oi.OrderId == o.OrderId),
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                }).ToList();

            var viewModel = new CustomerDashboardViewModel
            {
                UserName = user?.Name ?? "Customer",
                TotalOrders = userOrders.Count,
                TotalSpent = userOrders.Sum(o => o.TotalAmount),
                PendingOrders = userOrders.Count(o => o.Status == "Pending"),
                ProcessingOrders = userOrders.Count(o => o.Status == "Processing"),
                InTransitOrders = userOrders.Count(o => o.Status == "Shipped"),
                DeliveredOrders = userOrders.Count(o => o.Status == "Delivered"),
                RecentOrders = recentOrders
            };

            return View(viewModel);
        }
        public async Task<IActionResult> Browse(int? categoryId, string sort = "popular")
        {
            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();

            // Category filter
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId).ToList();

            // Sort
            products = sort switch
            {
                "price_asc" => products.OrderBy(p => p.Price).ToList(),
                "price_desc" => products.OrderByDescending(p => p.Price).ToList(),
                _ => products.OrderByDescending(p => p.ProductId).ToList()
            };

            var viewModel = products.Select(p => {
                var cat = categories.FirstOrDefault(c => c.CategoryId == p.CategoryId);
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

            // Dropdown ke liye
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedSort = sort;

            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");

            int userId = int.Parse(userIdClaim);

            // Pehle check karo — already cart mein hai?
            var allCarts = await _uow.Carts.GetAllAsync();
            var existing = allCarts.FirstOrDefault(c => c.UserId == userId
                                                     && c.ProductId == productId);
            if (existing != null)
            {
                // Already hai — quantity badhaao
                existing.Quantity += quantity;
                await _uow.Carts.UpdateAsync(existing);
            }
            else
            {
                // Naya cart item add karo
                await _uow.Carts.AddAsync(new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _uow.SaveAsync();

            TempData["Success"] = "✅ Product added to cart!";
            return RedirectToAction("Browse");
        }
        #region Cart
        // GET — Cart Page
        public async Task<IActionResult> Cart()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var allCarts = await _uow.Carts.GetAllAsync();
            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();

            var userCart = allCarts.Where(c => c.UserId == userId).ToList();

            var viewModel = new CartViewModel
            {
                Items = userCart.Select(c => {
                    var product = products.FirstOrDefault(p => p.ProductId == c.ProductId);
                    var cat = categories.FirstOrDefault(
                                    x => x.CategoryId == product?.CategoryId);
                    return new CartItemViewModel
                    {
                        CartId = c.CartId,
                        ProductId = c.ProductId,
                        ProductName = product?.Name ?? "N/A",
                        CategoryIcon = cat?.Icon ?? "📦",
                        UnitPrice = product?.Price ?? 0,
                        Quantity = c.Quantity,
                        Stock = product?.Stock ?? 0
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // POST — Quantity Update
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartId, string action)
        {
            var cart = await _uow.Carts.GetByIdAsync(cartId);
            if (cart == null) return RedirectToAction("Cart");

            var product = await _uow.Products.GetByIdAsync(cart.ProductId);

            if (action == "increase" && cart.Quantity < product.Stock)
            {
                cart.Quantity++;
                await _uow.Carts.UpdateAsync(cart);
                await _uow.SaveAsync();
            }
            else if (action == "decrease")
            {
                if (cart.Quantity > 1)
                {
                    cart.Quantity--;
                    await _uow.Carts.UpdateAsync(cart);
                    await _uow.SaveAsync();
                }
                else
                {
                    // Quantity 1 se kam ho toh remove karo
                    await _uow.Carts.DeleteAsync(cartId);
                    await _uow.SaveAsync();
                }
            }

            return RedirectToAction("Cart");
        }

        // POST — Remove Item
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            await _uow.Carts.DeleteAsync(cartId);
            await _uow.SaveAsync();

            TempData["Success"] = "Item removed from cart!";
            return RedirectToAction("Cart");
        }
        #endregion
        #region CheckOut
        // GET — Checkout Page
        public async Task<IActionResult> Checkout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var allCarts = await _uow.Carts.GetAllAsync();
            var userCart = allCarts.Where(c => c.UserId == userId).ToList();
            if (!userCart.Any()) return RedirectToAction("Browse");

            var products = await _uow.Products.GetAllAsync();
            var categories = await _uow.Categories.GetAllAsync();
            var user = await _uow.Users.GetByIdAsync(userId);

            var viewModel = new CheckoutViewModel
            {
                Items = userCart.Select(c => {
                    var product = products.FirstOrDefault(p => p.ProductId == c.ProductId);
                    var cat = categories.FirstOrDefault(
                                    x => x.CategoryId == product?.CategoryId);
                    return new CartItemViewModel
                    {
                        CartId = c.CartId,
                        ProductId = c.ProductId,
                        ProductName = product?.Name ?? "N/A",
                        CategoryIcon = cat?.Icon ?? "📦",
                        UnitPrice = product?.Price ?? 0,
                        Quantity = c.Quantity,
                        Stock = product?.Stock ?? 0
                    };
                }).ToList(),

                // ← User ka saved data pre-fill
                Phone = user?.Phone ?? "",
                Address = user?.Address ?? "",
                City = user?.City ?? "",
                Pincode = user?.Pincode ?? ""
            };

            return View(viewModel);
        }

        // POST — Place Order
        [HttpPost]

        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var allCarts = await _uow.Carts.GetAllAsync();
            var userCart = allCarts.Where(c => c.UserId == userId).ToList();
            if (!userCart.Any()) return RedirectToAction("Browse");

            var products = await _uow.Products.GetAllAsync();

            // ← User ka address update karo future ke liye
            var user = await _uow.Users.GetByIdAsync(userId);
            if (user != null)
            {
                user.Phone = model.Phone;
                user.Address = model.Address;
                user.City = model.City;
                user.Pincode = model.Pincode;
                await _uow.Users.UpdateAsync(user);
            }

            // Order banao
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Address = $"{model.Address}, {model.City} - {model.Pincode}",
                TotalAmount = userCart.Sum(c => {
                    var p = products.FirstOrDefault(x => x.ProductId == c.ProductId);
                    var subtotal = (p?.Price ?? 0) * c.Quantity;
                    return subtotal + Math.Round(subtotal * 0.18m, 2);
                })
            };

            await _uow.Orders.AddAsync(order);
            await _uow.SaveAsync();

            // OrderItems + Stock update + Cart clear
            foreach (var cartItem in userCart)
            {
                var product = products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                if (product == null) continue;

                await _uow.OrderItems.AddAsync(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price
                });

                product.Stock -= cartItem.Quantity;
                await _uow.Products.UpdateAsync(product);
                await _uow.Carts.DeleteAsync(cartItem.CartId);
            }

            await _uow.SaveAsync();

            TempData["OrderId"] = order.OrderId;
            TempData["Success"] = $"🎉 Order #ORD-{order.OrderId:D4} placed successfully!";
            return RedirectToAction("OrderSuccess");
        }

        // Order Success Page
        public IActionResult OrderSuccess()
        {
            if (TempData["OrderId"] == null)
                return RedirectToAction("Index");

            ViewBag.OrderId = TempData["OrderId"];
            ViewBag.Message = TempData["Success"];
            return View();
        }
        #endregion
        #region My Profile
        // GET — Profile Page
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var user = await _uow.Users.GetByIdAsync(userId);
            var allOrders = await _uow.Orders.GetAllAsync();
            var userOrders = allOrders.Where(o => o.UserId == userId).ToList();

            var viewModel = new ProfileViewModel
            {
                Name = user?.Name ?? "",
                Email = user?.Email ?? "",
                Phone = user?.Phone ?? "",
                Address = user?.Address ?? "",
                City = user?.City ?? "",
                Pincode = user?.Pincode ?? "",
                TotalOrders = userOrders.Count,
                TotalSpent = userOrders.Sum(o => o.TotalAmount)
            };

            return View(viewModel);
        }

        // POST — Profile Update
        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            if (!ModelState.IsValid) return View(model);

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return NotFound();

            // Update fields
            user.Name = model.Name;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;
            user.City = model.City;
            user.Pincode = model.Pincode;

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveAsync();

            TempData["UpdateSuccess"] = "✅ Profile updated successfully!";
            return RedirectToAction("Profile");
        }

        // POST — Change Password
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string? CurrentPassword,
                                                          string? NewPassword,
                                                          string? ConfirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var user = await _uow.Users.GetByIdAsync(userId);
            if (user == null) return NotFound();

            // Current password verify karo
            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(
                            user, user.PasswordHash, CurrentPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "❌ Current password is incorrect!";
                return RedirectToAction("Profile");
            }

            // New password match check
            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "❌ New passwords do not match!";
                return RedirectToAction("Profile");
            }

            // New password hash karo
            user.PasswordHash = hasher.HashPassword(user, NewPassword);
            await _uow.Users.UpdateAsync(user);
            await _uow.SaveAsync();

            TempData["Success"] = "✅ Password changed successfully!";
            return RedirectToAction("Profile");
        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> MyOrders(string status = "All")
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return RedirectToAction("Login", "Account");
            int userId = int.Parse(userIdClaim);

            var allOrders = await _uow.Orders.GetAllAsync();
            var allOrderItems = await _uow.OrderItems.GetAllAsync();
            var allProducts = await _uow.Products.GetAllAsync();
            var allCategories = await _uow.Categories.GetAllAsync();

            // User ke orders
            var userOrders = allOrders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            // Status filter
            if (status != "All")
                userOrders = userOrders.Where(o => o.Status == status).ToList();

            var viewModel = userOrders.Select(o => {
                // Is order ke items
                var orderItems = allOrderItems
                    .Where(oi => oi.OrderId == o.OrderId)
                    .ToList();

                // Pehla product
                var firstItem = orderItems.FirstOrDefault();
                var firstProduct = allProducts
                    .FirstOrDefault(p => p.ProductId == firstItem?.ProductId);
                var firstCat = allCategories
                    .FirstOrDefault(c => c.CategoryId == firstProduct?.CategoryId);

                return new MyOrderViewModel
                {
                    OrderId = o.OrderId,
                    FirstProductName = firstProduct?.Name ?? "Product",
                    FirstProductIcon = firstCat?.Icon ?? "📦",
                    TotalItems = orderItems.Count,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status
                };
            }).ToList();

            // Tab counts ke liye
            var allUserOrders = allOrders.Where(o => o.UserId == userId).ToList();
            ViewBag.AllCount = allUserOrders.Count;
            ViewBag.ProcessingCount = allUserOrders.Count(o => o.Status == "Processing");
            ViewBag.ShippedCount = allUserOrders.Count(o => o.Status == "Shipped");
            ViewBag.DeliveredCount = allUserOrders.Count(o => o.Status == "Delivered");
            ViewBag.CancelledCount = allUserOrders.Count(o => o.Status == "Cancelled");
            ViewBag.SelectedStatus = status;

            return View(viewModel);
        }

        // POST — Cancel Order
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            // Sirf Pending ya Processing order cancel ho sakta hai
            if (order.Status == "Pending" || order.Status == "Processing")
            {
                order.Status = "Cancelled";
                await _uow.Orders.UpdateAsync(order);
                await _uow.SaveAsync();
                TempData["Success"] = $"Order #ORD-{orderId:D4} cancelled!";
            }
            else
            {
                TempData["Error"] = "This order cannot be cancelled!";
            }

            return RedirectToAction("MyOrders");
        }
        public IActionResult Order_Track()
        {
            return View();
           
        }
        public IActionResult Reviews()
        {
            return View();
           
        }
        public IActionResult Notifications()
        {
            return View();
           
        }
        public IActionResult Payment()
        {
            return View();
           
        }
  
    }
}
