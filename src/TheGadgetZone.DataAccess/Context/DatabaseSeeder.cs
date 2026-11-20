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
