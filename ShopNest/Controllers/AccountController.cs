using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using ShopNest.Models;
using ShopNest.Models.ViewModels;
using ShopNest.Repositories.Interfaces;
using ShopNest.Services.AuthService;

namespace ShopNest.Controllers
{
    // Controllers/AccountController.cs
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly IUnitOfWork _uow;

        public AccountController(AuthService authService,IUnitOfWork uow)
        {
            _authService = authService;
            this._uow = uow;
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
                HttpOnly = true,
                Secure = false,    // ← HTTP ke liye false karo
                SameSite = SameSiteMode.Lax,  // ← Yeh add karo
                Expires = DateTime.UtcNow.AddHours(1)
            });


            // Role ke hisaab se redirect karo
            var role = _authService.GetRoleFromToken(token);

            if (role == "Admin")
                return RedirectToAction("Index", "Admin");
            else
                return RedirectToAction("Index", "Customer");
        }
        #region Forgot Password 
        // ── Forgot Password GET ───────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        // ── Forgot Password POST ──────────────────────────
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
                                          ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Email DB mein check karo
            var users = await _uow.Users.GetAllAsync();
            var user = users.FirstOrDefault(
                            u => u.Email.ToLower() == model.Email.ToLower());

            if (user == null)
            {
                // Security — user ko mat batao email exist karta hai ya nahi
                TempData["Info"] = "If this email exists, a reset link has been sent.";
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // ── Token Generate Karo ───────────────────────
            string token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30); // 30 min expiry

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveAsync();

            // ── Development Mode — Token Screen Pe Dikhao ─
            // (Baad mein yahan email bhejenge)
            TempData["ResetToken"] = token;
            TempData["ResetEmail"] = user.Email;

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // ── Forgot Password Confirmation ──────────────────
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ── Reset Password GET ────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            // Token valid hai?
            var users = await _uow.Users.GetAllAsync();
            var user = users.FirstOrDefault(
                            u => u.ResetToken == token &&
                            u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                TempData["Error"] = "❌ Reset link is invalid or expired!";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        // ── Reset Password POST ───────────────────────────
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(
                                          ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Token verify karo
            var users = await _uow.Users.GetAllAsync();
            var user = users.FirstOrDefault(
                            u => u.ResetToken == model.Token &&
                            u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                TempData["Error"] = "❌ Reset link is invalid or expired!";
                return RedirectToAction("ForgotPassword");
            }

            // ── Naya Password Set Karo ────────────────────
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
            user.ResetToken = null; // Token delete karo
            user.ResetTokenExpiry = null;

            await _uow.Users.UpdateAsync(user);
            await _uow.SaveAsync();

            TempData["Success"] = "✅ Password changed successfully! Please login.";
            return RedirectToAction("Login");
        }
        #endregion
        // ─── LOGOUT ─────────────────────────────────────

        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return RedirectToAction("Login");
        }
    }
}
