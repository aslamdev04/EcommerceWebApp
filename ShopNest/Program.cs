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

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Generic → sab entities ke liye
            builder.Services.AddScoped(typeof(IGenericRepository<>),
                                       typeof(GenericRepository<>));

            // Specific → sirf Product ke liye
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<AuthService>();
            //JWT Register
            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                builder.Configuration["JwtSettings:SecretKey"]))
                    };
                    //        // Cookie se token read karo
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request
                                                   .Cookies["AuthToken"]; // ← Cookie se read
                            return Task.CompletedTask;
                        }
                    };
                });
            var app = builder.Build();


        
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            // Middleware order important hai!
           
            app.UseRouting();
            app.UseAuthentication(); // ← Pehle
            app.UseAuthorization();  // ← Phir
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            // app.Run() se PEHLE yeh add karo
            using (var scope = app.Services.CreateScope())
            {
                await DbSeeder.SeedAdminAsync(scope.ServiceProvider);
            }

            app.Run();
            app.Run();
        }
    }
}
