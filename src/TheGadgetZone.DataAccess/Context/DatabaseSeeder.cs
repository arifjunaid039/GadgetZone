using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheGadgetZone.Business.Entities;
using TheGadgetZone.DataAccess.Identity;

namespace TheGadgetZone.DataAccess.Context;

public static class DatabaseSeeder
{
    private static readonly string[] CategoryNames =
    {
        "Mobile Accessories",
        "Earbuds",
        "Smartwatches",
        "Chargers and Cables",
        "Keyboards and Mouse",
        "Laptop Bags",
        "Gaming Accessories",
        "Smart Gadgets"
    };

    private static readonly (string Category, string Name, string Description, decimal Price, int Stock, string ImageUrl)[] SampleProducts =
    {
        ("Mobile Accessories", "MagSafe Fast Wireless Charger", "A slim magnetic wireless charger for compatible smartphones, designed for reliable everyday charging.", 3499m, 24, "https://images.unsplash.com/photo-1591290619762-c588fc6c9c28?auto=format&fit=crop&w=900&q=85"),
        ("Mobile Accessories", "Braided USB-C Fast Charging Cable", "A durable, tangle-resistant USB-C cable for fast charging and everyday data transfer.", 899m, 45, "https://images.unsplash.com/photo-1572569511254-d8f925fe2cbb?auto=format&fit=crop&w=900&q=85"),
        ("Earbuds", "Pulse Pro Wireless Earbuds", "Compact wireless earbuds with a comfortable fit, clear calls, and a pocket-sized charging case.", 5499m, 18, "https://images.unsplash.com/photo-1606220945770-b5b6c2c55bf1?auto=format&fit=crop&w=900&q=85"),
        ("Earbuds", "Studio Bass Bluetooth Headphones", "Over-ear wireless headphones with rich bass and cushioned ear cups for long listening sessions.", 7999m, 12, "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&q=85"),
        ("Smartwatches", "Active Fit Smartwatch", "A bright-touchscreen smartwatch with activity tracking, notifications, and all-day comfort.", 8999m, 14, "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=900&q=85"),
        ("Smartwatches", "Classic Steel Smartwatch Strap", "A polished replacement strap that gives your compatible smartwatch a clean, premium finish.", 2199m, 20, "https://images.unsplash.com/photo-1434493789847-2f02dc6ca35d?auto=format&fit=crop&w=900&q=85"),
        ("Chargers and Cables", "Dual-Port 30W Wall Charger", "A compact dual-port wall adapter for charging your phone and accessories at the same time.", 2799m, 22, "https://images.unsplash.com/photo-1609091839311-d5365f9ff1c5?auto=format&fit=crop&w=900&q=85"),
        ("Chargers and Cables", "3-in-1 Charging Cable", "One convenient cable with interchangeable connectors for your everyday devices.", 1199m, 30, "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?auto=format&fit=crop&w=900&q=85"),
        ("Keyboards and Mouse", "Mechanical RGB Gaming Keyboard", "A responsive mechanical keyboard with vibrant backlighting for gaming and productive work.", 8499m, 10, "https://images.unsplash.com/photo-1587829741301-dc798b83add3?auto=format&fit=crop&w=900&q=85"),
        ("Keyboards and Mouse", "Ergonomic Wireless Mouse", "A comfortable wireless mouse with precise tracking for work, study, and everyday browsing.", 2499m, 17, "https://images.unsplash.com/photo-1527814050087-3793815479db?auto=format&fit=crop&w=900&q=85"),
        ("Laptop Bags", "Everyday Laptop Backpack", "A water-resistant backpack with a padded laptop compartment and room for daily essentials.", 6999m, 9, "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?auto=format&fit=crop&w=900&q=85"),
        ("Laptop Bags", "Slim Laptop Sleeve 15-inch", "A lightweight padded sleeve that helps protect your laptop while commuting or travelling.", 2999m, 13, "https://images.unsplash.com/photo-1546938576-6e6a64f317cc?auto=format&fit=crop&w=900&q=85"),
        ("Gaming Accessories", "RGB Gaming Headset", "An over-ear gaming headset with a flexible microphone and immersive stereo sound.", 6499m, 11, "https://images.unsplash.com/photo-1599669454699-248893623440?auto=format&fit=crop&w=900&q=85"),
        ("Gaming Accessories", "Precision Gaming Mouse Pad", "A smooth, generously sized surface for consistent mouse control during intense sessions.", 1799m, 19, "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?auto=format&fit=crop&w=900&q=85"),
        ("Smart Gadgets", "Portable Bluetooth Speaker", "A compact wireless speaker that brings clear sound to your desk, room, or outdoor plans.", 4599m, 15, "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?auto=format&fit=crop&w=900&q=85"),
        ("Smart Gadgets", "Smart LED Desk Lamp", "A modern desk lamp with adjustable brightness to create a comfortable workspace.", 3899m, 8, "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?auto=format&fit=crop&w=900&q=85")
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        foreach (var name in CategoryNames)
        {
            if (!await dbContext.Categories.AnyAsync(category => category.Name == name))
            {
                dbContext.Categories.Add(new Category { Name = name });
            }
        }

        await dbContext.SaveChangesAsync();
        await SeedProductsAsync(dbContext);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await EnsureRoleAsync(roleManager, "Customer");
        await EnsureRoleAsync(roleManager, "Admin");

        var email = configuration["InitialAdmin:Email"];
        var password = configuration["InitialAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) != string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Set both InitialAdmin:Email and InitialAdmin:Password to provision the initial admin.");
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = "Store Administrator",
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not create initial admin: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Could not assign the Admin role: {string.Join("; ", roleResult.Errors.Select(error => error.Description))}");
            }
        }
    }

    private static async Task SeedProductsAsync(ApplicationDbContext dbContext)
    {
        var categoryIds = await dbContext.Categories
            .ToDictionaryAsync(category => category.Name, category => category.Id);
        var existingProducts = await dbContext.Products
            .Select(product => new { product.CategoryId, product.Name })
            .ToListAsync();
        var existingProductKeys = existingProducts
            .Select(product => (product.CategoryId, product.Name))
            .ToHashSet();

        foreach (var sample in SampleProducts)
        {
            var categoryId = categoryIds[sample.Category];
            if (!existingProductKeys.Add((categoryId, sample.Name)))
            {
                continue;
            }

            dbContext.Products.Add(new Product
            {
                CategoryId = categoryId,
                Name = sample.Name,
                Description = sample.Description,
                Price = sample.Price,
                StockQuantity = sample.Stock,
                ImageUrl = sample.ImageUrl,
                IsActive = true
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Could not create the {roleName} role: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
