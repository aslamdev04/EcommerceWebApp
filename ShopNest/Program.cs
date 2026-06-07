using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopNest.Data;
using ShopNest.Repositories.Implementations;
using ShopNest.Repositories.Interfaces;
using ShopNest.Services.AuthService;
using System.Text;

namespace ShopNest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Services ──────────────────────────────────
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration
                           .GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped(
                typeof(IGenericRepository<>),
                typeof(GenericRepository<>));

            builder.Services.AddScoped<IProductRepository,
                                       ProductRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<AuthService>();

            // ── JWT ───────────────────────────────────────
            // ← Null safe tarika
            var secretKey = builder.Configuration["JwtSettings:SecretKey"]
                            ?? "DefaultSecretKey1234567890123456";
            var issuer = builder.Configuration["JwtSettings:Issuer"]
                            ?? "EcommerceApp";
            var audience = builder.Configuration["JwtSettings:Audience"]
                            ?? "EcommerceUsers";

            builder.Services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = issuer,
                            ValidAudience = audience,
                            IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(secretKey))
                        };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request
                                                   .Cookies["AuthToken"];
                            return Task.CompletedTask;
                        }
                    };
                });

            // ── Port Configuration ────────────────────────
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            builder.WebHost.UseUrls($"http://*:{port}");

            // ── Build ─────────────────────────────────────
            var app = builder.Build();

            // ── Seed Data ─────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    await DbSeeder.SeedAdminAsync(scope.ServiceProvider);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Seed Error: {ex.Message}");
                }
            }

            // ── Middleware ────────────────────────────────
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}