using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iBorrow.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";
    public const string StudentRole = "Student";

    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var db = provider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AdminRole, StudentRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            return;

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var existingAdmins = await userManager.GetUsersInRoleAsync(AdminRole);
        var admin = existingAdmins.FirstOrDefault();

        foreach (var extra in existingAdmins.Skip(1))
            await userManager.DeleteAsync(extra);

        if (admin is null)
        {
            admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            await userManager.AddToRoleAsync(admin, AdminRole);
            return;
        }

        if (!string.Equals(admin.Email, adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            admin.UserName = adminEmail;
            admin.Email = adminEmail;
            admin.EmailConfirmed = true;
            await userManager.UpdateAsync(admin);
        }

        if (!await userManager.CheckPasswordAsync(admin, adminPassword))
        {
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(admin);
            await userManager.ResetPasswordAsync(admin, resetToken, adminPassword);
        }
    }
}
