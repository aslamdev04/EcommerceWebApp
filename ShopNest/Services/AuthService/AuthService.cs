using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ShopNest.Models;
using ShopNest.Repositories.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShopNest.Services.AuthService
{
    // Services/AuthService.cs
    public class AuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _hasher = new();

        public AuthService(IUnitOfWork uow, IConfiguration config)
        {
            _uow = uow;
            _config = config;
        }
        // Services/AuthService.cs mein add karo
        public string GetRoleFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims
                      .FirstOrDefault(c => c.Type == ClaimTypes.Role)
                      ?.Value;
        }

        // Register
        public async Task<bool> RegisterAsync(string name,
                                              string email,
                                              string password,
                                              string role = "User")
        {
            // Email duplicate check
            var users = await _uow.Users.GetAllAsync();
            if (users.Any(u => u.Email == email)) return false; // ← Yeh add karo

            var user = new User
            {
                Name = name,
                Email = email,
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            // Password hash karo
            user.PasswordHash = _hasher.HashPassword(user, password);

            await _uow.Users.AddAsync(user);
            await _uow.SaveAsync();
            return true;
        }

        // Login → JWT Token return karta hai
        public async Task<string> LoginAsync(string email, string password)
        {
            var users = await _uow.Users.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Email == email);

            if (user == null) return null;

            // Password verify karo
            var result = _hasher.VerifyHashedPassword(
                            user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Failed) return null;

            // JWT Token banao
            return GenerateToken(user);
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["JwtSettings:SecretKey"]));

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier,
                      user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role), // ← Role yahan!
            new Claim(ClaimTypes.Name, user.Name)
        };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
