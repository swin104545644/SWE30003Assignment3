using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("OnlineShopDb"));

builder.Services.AddSession();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData(db);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();

// Program.cs – inside SeedData()
void SeedData(AppDbContext context)
{
    // Create default admin if no users exist
    if (!context.Users.Any())
    {
        var admin = new User
        {
            Name = "admin",
            Email = "admin@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
            IsAdmin = true
        };
        context.Users.Add(admin);
    }

    // Seed products if none exist
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Name = "Laptop", Description = "High-performance laptop", Price = 999.99m, ImageUrl = "https://via.placeholder.com/300", Stock = 5 },
            new Product { Name = "Mouse", Description = "Wireless mouse", Price = 25.50m, ImageUrl = "https://via.placeholder.com/300", Stock = 0 },
            new Product { Name = "Keyboard", Description = "Mechanical keyboard", Price = 89.99m, ImageUrl = "https://via.placeholder.com/300", Stock = 3 }
        );
    }

    context.SaveChanges();
}