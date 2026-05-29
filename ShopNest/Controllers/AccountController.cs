using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNest.Models.ViewModels;
using ShopNest.Services.AuthService;

namespace ShopNest.Controllers
{
    // Controllers/AccountController.cs
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // ─── REGISTER ───────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var success = await _authService.RegisterAsync(
                                model.Name,
                                model.Email,
                                model.Password);

            if (!success)
            {
                ModelState.AddModelError("", "Email already exists!");
                return View(model);
            }

            return RedirectToAction("Login");
        }

        // ─── LOGIN ──────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var token = await _authService.LoginAsync(
                              model.Email,
                              model.Password);

            if (token == null)
            {
                ModelState.AddModelError("", "Invalid email or password!");
                return View(model);
            }

            // Token cookie mein store karo
            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,     // JS se access nahi hoga
                Secure = true,       // Sirf HTTPS pe
                Expires = DateTime.UtcNow.AddDays(7)
            });

            // Role ke hisaab se redirect karo
            var role = _authService.GetRoleFromToken(token);

            if (role == "Admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Home");
        }

        // ─── LOGOUT ─────────────────────────────────────

        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }
}
